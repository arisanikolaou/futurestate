using System;
using System.Collections.Generic;

namespace FutureState.Flow.Flow
{
    /// <summary>
    /// 
    /// </summary>
    public class FlowFileLog
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog()
        {
            Entries = new List<FlowFileLogEntry>();
            ProcessId = SeqGuid.Create();
        }

        public Guid ProcessId { get; set; }

        public int BatchId { get; set; }


        public List<FlowFileLogEntry> Entries { get; set; }
    }
}