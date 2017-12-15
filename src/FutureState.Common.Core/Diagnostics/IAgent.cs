namespace FutureState.Diagnostics
{
    /// <summary>
    /// A 'micro' service that can be started and stopped and composed within a given
    /// windows service.
    /// </summary>
    /// <remarks>
    /// Only one instance is expected to run in a given application.
    /// </remarks>
    public interface IAgent
    {
        /// <summary>
        /// Gets whether the instance has started.
        /// </summary>
        bool HasStarted { get; }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <remarks>
        /// Will throw an invalid operation if already started.
        /// </remarks>
        void Start();

        /// <summary>
        /// Stops a started agent. If the agent was not started this will not throw an exception.
        /// </summary>
        void Stop();
    }
}