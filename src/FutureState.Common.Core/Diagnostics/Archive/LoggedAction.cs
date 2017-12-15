#region

using System;
using System.Diagnostics;
using NLog;

#endregion

namespace FutureState.Diagnostics
{
    // TODO: replace with postsharpe
    /// <summary>
    /// An action whose start and end are traced through a given logger.
    /// </summary>
    public class LoggedAction : ManagedActionBase
    {
        private new static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Emits a start/stop logging wrapper around the action.
        /// </summary>
        public static void Execute(Action action, string name = null, object tag = null)
        {
            var loggedAction = new LoggedAction();
            loggedAction.Setup(action, name, tag);
            loggedAction.Invoke();
        }

        /// <summary>
        /// Emits the actions total elapsed time with the logging headers in addition to a start/stop
        /// logging wrapper.
        /// </summary>
        public static void ExecuteTimed(Action action, string name = "Action", object tag = null)
        {
            var loggedAction = new LoggedAction();
            loggedAction.Setup(action, name, tag);

            var sw = new Stopwatch();
            sw.Start();
            try
            {
                loggedAction.Invoke();
            }
            finally
            {
                _logger.Trace("Action {0} total time: {0}", (object) name, (object) sw.Elapsed);
            }
        }

        /// <summary>
        /// Executes the action passed to the setup method.
        /// </summary>
        protected override void EndInvoke()
        {
            if (_action == null)
            {
                throw new InvalidOperationException("The underlying action to invoke has not been setup.");
            }

            // todo: add instrumentation
            _logger.Trace("Starting {0}", Name);

            try
            {
                _action.Invoke();
            }
            catch (Exception ex)
            {
                var msg = "Failed executing {0}: {1}".Params(Name, ex);
                _logger.Error(msg);

                throw;
            }
            finally
            {
                _logger.Trace("Finished: {0}", Name);
            }
        }
    }

    public static class LoggedActionEx
    {
        /// <summary>
        /// Executes a given action and times the duration of the operation. The results are recorded to
        /// trace log output.
        /// </summary>
        public static void ExecTimed(this Action action, string name, object tag = null)
        {
            LoggedAction.ExecuteTimed(action, name, tag);
        }
    }
}