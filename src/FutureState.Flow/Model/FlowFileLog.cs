using System.Collections.Generic;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     A log of all the flow files processed for a particular flow.
    /// </summary>
    public class FlowFileLog
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog(string flowCode)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(flowCode, nameof(flowCode));

            Entries = new List<FlowFileLogEntry>();
            FlowCode = flowCode;
        }

        /// <summary>
        ///     Gets/sets the flow code to use.
        /// </summary>
        public string FlowCode { get; set; }

        /// <summary>
        ///     Gets the flow batch id.
        /// </summary>
        public int BatchId { get; set; }

        /// <summary>
        ///     Gets the flow file log entries to record which flow files were process againts a flow.
        /// </summary>
        public List<FlowFileLogEntry> Entries { get; set; }
    }
}