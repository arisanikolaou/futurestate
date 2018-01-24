using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     Represents a batch process or BatchProcess used in a process flow.
    /// </summary>
    public class BatchProcess
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public BatchProcess()
        {
            // required for serializer
        }

        /// <summary>
        ///     Creates a new batch instance against a given process id and a batch id.
        /// </summary>
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