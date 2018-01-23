using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     Represents a batch process or BatchProcess used in a process flow.
    /// </summary>
    public class BatchProcess
    {
        public BatchProcess()
        {
            // required for serializer
        }

        public BatchProcess(Guid processId, int batchId)
        {
            ProcessId = processId;
            BatchId = batchId;
        }

        /// <summary>
        ///     Gets the process id.
        /// </summary>
        public Guid ProcessId { get; set; }

        /// <summary>
        ///     Gets the job/batch number.
        /// </summary>
        public long BatchId { get; set; }
    }
}