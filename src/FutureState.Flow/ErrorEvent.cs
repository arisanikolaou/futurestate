namespace FutureState.Flow
{
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