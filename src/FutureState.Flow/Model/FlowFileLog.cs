using System;
using System.Collections.Generic;

namespace FutureState.Flow.Model
{
    /// <summary>
    /// 
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