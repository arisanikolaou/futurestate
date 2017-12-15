namespace FutureState.Diagnostics
{
    /// <summary>
    /// A critical action that is instrumented with either with logging and/or performance counters.
    /// </summary>
    public interface IManagedAction
    {
        /// <summary>
        /// The unit of work being managed.
        /// </summary>
        void Invoke();
    }
}