namespace FutureState.Flow.Model
{
    public class FlowFileLogEntry
    {
        /// <summary>
        ///     Gets the id of the processor that produce the flow file.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///     Gets the batch id of the flow file that was processed.
        /// </summary>
        public int BatchId { get; set; }

        /// <summary>
        ///     Gets the identifier of the flow file processed.
        /// </summary>
        public string FlowFileProcessed { get; set; }
    }
}