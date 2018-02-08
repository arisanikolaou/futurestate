using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     Represents a batch process or BatchProcess used in a process flow.
    /// </summary>
    public class BatchProcess : IEquatable<BatchProcess>
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
        public BatchProcess(Guid flowId, long batchId)
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

        /// <summary>
        ///     Compares one batch to another for value equality.
        /// </summary>
        public bool Equals(BatchProcess other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return FlowId.Equals(other.FlowId) && BatchId == other.BatchId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BatchProcess) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (FlowId.GetHashCode() * 397) ^ BatchId.GetHashCode();
            }
        }

        public BatchProcess Increment()
        {
            return new BatchProcess(FlowId, BatchId + 1);
        }
    }
}