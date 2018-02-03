using System;
using System.Collections.Generic;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     A log of all the flow file processed in a distinct flow id.
    /// </summary>
    public class FlowFileLog
    {
        public FlowFileLog()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog(Guid flowId)
        {
            Entries = new List<FlowFileLogEntry>();
            FlowId = flowId;
        }

        public Guid FlowId { get; set; }

        public int BatchId { get; set; }

        public List<FlowFileLogEntry> Entries { get; set; }
    }
}