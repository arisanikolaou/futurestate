namespace FutureState
{
    /// <summary>
    /// Used by any dto structure used to communicate error messages.
    /// </summary>
    /// <remarks>
    /// To string should also be used.
    /// </remarks>
    public interface IExceptionData
    {
        /// <summary>
        /// The underlying error message.
        /// </summary>
        string Message { get; }
    }
}