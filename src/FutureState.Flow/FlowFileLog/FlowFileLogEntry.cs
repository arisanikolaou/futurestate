namespace FutureState.Flow.Model
{
    /// <summary>
    ///     Logs the flow file snapshots processed.
    /// </summary>
    public class FlowFileLogEntry
    {
        /// <summary>
        ///     Gets the batch id of the flow file that was processed.
        /// </summary>
        public long BatchId { get; set; }

        /// <summary>
        ///     Gets the identifier of the primary source file used to create the output.
        /// </summary>
        public string SourceAddressId { get; set; }

        /// <summary>
        ///     Gets the identifier of the flow file processed.
        /// </summary>
        public string TargetAddressId { get; set; }

        /// <summary>
        ///     Gets the target entity type created.
        /// </summary>
        public FlowEntity TargetEntityType { get; set; }

        /// <summary>
        ///     Gets the primary source entity type used to produce the target entity type.
        /// </summary>
        public FlowEntity SourceEntityType { get; set; }
    }
}