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
        public BatchProcess(Guid flowId, int batchId)
        {
            FlowId = flowId;
            BatchId = batchId;
        }

        /// <summary>
        ///     Gets the process id.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        ///     Gets the job/batch number.
        /// </summary>
        public long BatchId { get; set; }
    }
}