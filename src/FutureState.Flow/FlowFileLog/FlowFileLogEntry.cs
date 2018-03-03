using System;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     Logs the flow file snapshots processed.
    /// </summary>
    public class FlowFileLogEntry : DataFileLogEntry
    {
        /// <summary>
        ///     Gets the batch id of the flow file that was processed.
        /// </summary>
        public long BatchId { get; set; }

        /// <summary>
        ///     Gets the identifier of the flow file processed. This is typically a file path.
        /// </summary>
        public string TargetAddressId { get; set; }
    }
}