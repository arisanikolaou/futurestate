#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace FutureState
{
    /// <summary>
    ///     Extensions to how the system can raise events.
    /// </summary>
    public static class EventHandlerExtensions
    {
        [DebuggerStepThrough]
        public static async Task AsyncRaiseSafe<T>(this EventHandler<T> evt, object sender, T e,
            Action<Exception> exceptionHandler = null) where T : EventArgs
        {
            if (evt == null)
                return;

            var tasks = new List<Task>();

            // save target invocation list here
            lock (evt)
            {
                var methods = evt.GetInvocationList().ToArray();
                
                for (var i = 0; i < methods.Length; i++)
                {
                    // ReSharper disable once UsePatternMatching
                    var innerMethod = methods[i] as EventHandler<T>;

                    if (innerMethod != null)
                    {
                        tasks.Add(Task.Run(
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
                            }));
                    }
                }
            }

            await Task.WhenAll(tasks);
        }

        [DebuggerStepThrough]
        public static async Task AsyncRaiseSafe(this EventHandler evt, object sender, EventArgs e,
            Action<Exception> exceptionHandler = null)
        {
            if (evt == null)
                return;

            // save target invocation list here
            var methods = evt.GetInvocationList().ToArray();

            var tasks = new List<Task>();

            for (var i = 0; i < methods.Length; i++)
            {
                // ReSharper disable once UsePatternMatching
                var innerMethod = methods[i] as EventHandler;

                if (innerMethod != null)
                {
                    tasks.Add(Task.Run(
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
                        }));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        ///     Raises an event in a thread safe way and ensures that all subscribers are notified.
        /// </summary>
        [DebuggerStepThrough]
        public static void RaiseSafe(this EventHandler del, object sender, EventArgs e,
            Action<Exception> exceptionHandler = null)
        {
            var evt = del;

            if (evt == null)
                return;

            // save target invocation list here
            var methods = evt.GetInvocationList().ToArray();

            for (var i = 0; i < methods.Length; i++)
            {
                // ReSharper disable once UsePatternMatching
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

        /// <summary>
        ///     Raises an event in a thread safe way and ensures that all subscribers are notified.
        /// </summary>
        [DebuggerStepThrough]
        public static void RaiseSafe<T>(this EventHandler<T> del, object sender, T e,
            Action<Exception> exceptionHandler = null) where T : EventArgs
        {
            var evt = del;

            if (evt == null)
                return;

            // save target invocation list here
            var methods = evt.GetInvocationList().ToArray();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < methods.Length; i++)
            {
                // ReSharper disable once UsePatternMatching
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