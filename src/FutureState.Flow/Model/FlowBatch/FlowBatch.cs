using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A batch process identifier associated with a given flow.
    /// </summary>
    public class FlowBatch : IEquatable<FlowBatch>
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowBatch()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowBatch(Flow flow, long batchId)
        {
            Guard.ArgumentNotNull(flow, nameof(flow));

            Flow = flow;
            BatchId = batchId;
        }

        /// <summary>
        ///     The originating flow.
        /// </summary>
        public Flow Flow { get; set; }

        /// <summary>
        ///     Gets the batch id.
        /// </summary>
        public long BatchId { get; set; }

        /// <summary>
        ///     Compares one batch to another for value equality.
        /// </summary>
        public bool Equals(FlowBatch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Flow.Equals(other.Flow) && BatchId == other.BatchId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((FlowBatch)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Flow.Code.GetHashCode() * 397) ^ BatchId.GetHashCode();
            }
        }
    }

    /// <summary>
    ///     A log of the batch process.
    /// </summary>
    public class FlowBatchLog
    {
        public FlowBatch Batch { get; set; }

        public long Processed { get; set; }

        public long Errors { get; set; }
    }
}