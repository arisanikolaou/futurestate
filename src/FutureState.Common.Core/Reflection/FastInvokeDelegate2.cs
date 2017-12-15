#region

using System;
using System.Threading;

#endregion

namespace FutureState
{
    //originally taken from http://www.codeproject.com/Articles/37788/Fast-Asynchronous-Delegates-in-NET

    /// <summary>
    /// Delegate used to implement async programming model leveraging the new TPL library.
    /// </summary>
    public delegate TResult FastInvokeDelegate<out TResult>();

    /// <summary>
    /// Extensions methods to FastInvokeDelegate to support async programming model.
    /// </summary>
    public static class FastInvokeDelegateEx
    {
        /// <summary>
        /// Begins an async operation represented by a delegate passing in state and completing the operation calling a given
        /// callback method.
        /// </summary>
        public static IAsyncResult BeginInvokeFast<TResult>(this FastInvokeDelegate<TResult> del, object state,
            AsyncCallback callback)
        {
            return new FastInvokeAsyncResult<TResult>(del, callback, state);
        }

        /// <summary>
        /// Completes an async operation.
        /// </summary>
        public static TResult EndInvokeFast<TResult>(this FastInvokeDelegate<TResult> del, IAsyncResult asyncResult)
        {
            var result = asyncResult as FastInvokeAsyncResult<TResult>;

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"The async result was not the expected type: {typeof(FastInvokeAsyncResult<TResult>).FullName} but {asyncResult.GetType().FullName}");
            }

            return result.End();
        }

        private class FastInvokeAsyncResult<T> : IAsyncResult
        {
            private readonly AsyncCallback _callback;

            private ManualResetEvent _asyncWaitHandle;

            private volatile int _asyncWaitHandleNeeded; //0 - is not needed, 1 - needed

            // To hold the results, exceptional or ordinary.
            private Exception _exception;

            private volatile int _isCompleted; // 0==not complete, 1==complete.

            private T m_result;

            public FastInvokeAsyncResult(FastInvokeDelegate<T> work, AsyncCallback callback, object state)
            {
                _callback = callback;
                AsyncState = state;

                Run(work);
            }

            public object AsyncState { get; }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (_asyncWaitHandleNeeded == 1)
                    {
                        return _asyncWaitHandle;
                    }
                    _asyncWaitHandleNeeded = 1;
                    _asyncWaitHandle = new ManualResetEvent(_isCompleted == 1);

                    return _asyncWaitHandle;
                }
            }

            public bool CompletedSynchronously => false;

            public bool IsCompleted => _isCompleted == 1;

            public T End()
            {
                if (_isCompleted == 0)
                {
                    AsyncWaitHandle.WaitOne();
                    AsyncWaitHandle.Close();
                }

                if (_exception != null)
                {
                    throw _exception;
                }
                return m_result;
            }

            private void Run(FastInvokeDelegate<T> work)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        try
                        {
                            m_result = work();
                        }
                        catch (Exception e)
                        {
                            _exception = e;
                        }
                        finally
                        {
                            _isCompleted = 1;
                            if (_asyncWaitHandleNeeded == 1)
                            {
                                _asyncWaitHandle.Set();
                            }
                            if (_callback != null)
                            {
                                _callback(this);
                            }
                        }
                    });
            }
        }
    }
}