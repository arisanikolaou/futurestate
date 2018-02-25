namespace FutureState.Flow
{
    /// <summary>
    ///     A generic error raised while processing data.
    /// </summary>
    public class ErrorEvent
    {
        /// <summary>
        ///     Gets the type of error event.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Gets the error event message.
        /// </summary>
        public string Message { get; set; }
    }
}