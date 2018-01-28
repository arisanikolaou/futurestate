namespace FutureState.Flow
{
    /// <summary>
    ///     Process data from a given incoming source.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        ///     Gets the process display name.
        /// </summary>
        string ProcessName { get; }
    }
}