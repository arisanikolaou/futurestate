namespace FutureState.Flow
{
    /// <summary>
    ///     Process data from a given incoming source.
    /// </summary>
    public interface IProcessor
    {
        string ProcessName { get; }
    }
}