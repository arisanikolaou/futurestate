namespace FutureState.Flow.Model
{
    public class FlowFileLogEntry
    {
        /// <summary>
        ///     Gets the id of the processor that
        /// </summary>
        public string ControllerName { get; set; }

        public int BatchId { get; set; }

        /// <summary>
        ///     Gets the identifier of the flow file processed.
        /// </summary>
        public string FlowFileProcessed { get; set; }
    }
}