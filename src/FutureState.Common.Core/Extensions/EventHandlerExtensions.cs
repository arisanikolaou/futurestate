#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace FutureState
{
    /// <summary>
    ///     Code automation class to raise thread safe events.
    /// </summary>
    public static class EventHandlerExtensions
    {
        [DebuggerStepThrough]
        public static void AsyncRaiseSafe<T>(this EventHandler<T> del, object sender, T e,
            Action<Exception> exceptionHandler = null) where T : EventArgs
        {
            var evt = del;

            if (evt != null)
            {
                // save target invocation list here
                var methods = evt.GetInvocationList().ToArray();

                for (var i = 0; i < methods.Length; i++)
                {
                    var innerMethod = methods[i] as EventHandler<T>;

                    if (innerMethod != null)
                        Task.Factory.StartNew(
                            () =>
                            {
                                try
                                {
                                    innerMethod.Invoke(sender, e);
                                }
                                catch (Exception ex)
                                {
                                    if (exceptionHandler != null)
                                        exceptionHandler(ex);
                                    else
                                        throw;
                                }
                            });
                }
            }
        }

        [DebuggerStepThrough]
        public static void AsyncRaiseSafe(this EventHandler del, object sender, EventArgs e,
            Action<Exception> exceptionHandler = null)
        {
            var evt = del;

            if (evt != null)
            {
                // save target invocation list here
                var methods = evt.GetInvocationList().ToArray();

                for (var i = 0; i < methods.Length; i++)
                {
                    var innerMethod = methods[i] as EventHandler;

                    if (innerMethod != null)
                        Task.Factory.StartNew(
                            () =>
                            {
                                try
                                {
                                    innerMethod.Invoke(sender, e);
                                }
                                catch (Exception ex)
                                {
                                    if (exceptionHandler != null)
                                        exceptionHandler(ex);
                                    else
                                        throw;
                                }
                            });
                }
            }
        }

        /// <summary>
        ///     Raises an event in a thread safe wayd.
        /// </summary>
        public static void Raise(this EventHandler del, object sender, EventArgs e)
        {
            var evt = del;

            if (evt != null)
                evt.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises an event in a thread safe way.
        /// </summary>
        public static void Raise<T>(this EventHandler<T> del, object sender, T e) where T : EventArgs
        {
            var evt = del;

            if (evt != null)
                evt.Invoke(sender, e);
        }

        /// <summary>
        ///     Raises an event in a thread safe way and ensures that all subscribers are notified.
        /// </summary>
        [DebuggerStepThrough]
        public static void RaiseSafe(this EventHandler del, object sender, EventArgs e,
            Action<Exception> exceptionHandler = null)
        {
            var evt = del;

            if (evt != null)
            {
                // save target invocation list here
                var methods = evt.GetInvocationList().ToArray();

                for (var i = 0; i < methods.Length; i++)
                {
                    var innerMethod = methods[i] as EventHandler;

                    if (innerMethod != null)
                        try
                        {
                            innerMethod.Invoke(sender, e);
                        }
                        catch (Exception ex)
                        {
                            if (exceptionHandler != null)
                                exceptionHandler(ex);
                            else
                                throw;
                        }
                }
            }
        }

        /// <summary>
        ///     Raises an event in a thread safe way and ensures that all subscribers are notified.
        /// </summary>
        [DebuggerStepThrough]
        public static void RaiseSafe<T>(this EventHandler<T> del, object sender, T e,
            Action<Exception> exceptionHandler = null) where T : EventArgs
        {
            var evt = del;

            if (evt != null)
            {
                // save target invocation list here
                var methods = evt.GetInvocationList().ToArray();

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < methods.Length; i++)
                {
                    var innerMethod = methods[i] as EventHandler<T>;

                    if (innerMethod != null)
                        try
                        {
                            innerMethod.Invoke(sender, e);
                        }
                        catch (Exception ex)
                        {
                            if (exceptionHandler != null)
                                exceptionHandler(ex);
                            else
                                throw;
                        }
                }
            }
        }
    }
}