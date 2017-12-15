#region

using NLog;
using System;
using System.Threading;

#endregion

namespace FutureState
{
    /// <summary>
    /// A durable action that should be re-executed until either a maximum
    /// retry attempt is exceeded or a retry condition is not met.
    /// </summary>
    public class DurableAction
    {
        private static readonly Logger _logger = LogManager.GetLogger(nameof(DurableAction));

        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="DurableAction" /> class.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public DurableAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            _action = action;
        }

        /// <summary>
        /// Gets the number of execution attempts made to run the durable action.
        /// </summary>
        public int ExecutionAttempts { get; private set; }

        /// <summary>
        /// Gets the last exception that was raised.
        /// </summary>
        public Exception LastException { get; private set; }

        /// <summary>
        /// Tries to execute a unit of work for a maximum number of attempts before error-ing out.
        /// </summary>
        public void Invoke(int maxRetries, int seconds, Predicate<Exception> waitCondition = null)
        {
            Invoke(maxRetries, TimeSpan.FromSeconds(seconds), waitCondition);
        }

        /// <summary>
        /// Tries to execute a unit of work for a maximum number of attempts before erroring out.
        /// </summary>
        /// <param name="waitCondition">The condition to use to wait and repeat the action.</param>
        /// <param name="maxRetries">The max retries before error out.</param>
        /// <param name="waitTimeOut">The wait time out.</param>
        public void Invoke(int maxRetries, TimeSpan waitTimeOut, Predicate<Exception> waitCondition = null)
        {
            ExecutionAttempts = 0;

            for (var i = 1; i <= maxRetries; i++)
            {
                try
                {
                    _action.Invoke();

                    // always break if successful
                    // -------------------------------------------
                    break;

                    // -------------------------------------------
                }
                catch (Exception ex)
                {
                    LastException = ex;

                    if (i < maxRetries && (waitCondition == null || waitCondition(ex)))
                    {
                        _logger.Debug(
                            "Failed to execute {0}. Waiting {1} to retry. Attempt {2}",
                            _action.Method.Name,
                            waitTimeOut,
                            i);

                        Thread.Sleep(waitTimeOut);

                        ExecutionAttempts++;
                    }
                    else
                    {
                        // error out
                        throw;
                    }
                }
            }
        }
    }
}