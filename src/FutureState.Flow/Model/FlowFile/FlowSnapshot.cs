using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     A snapshot of data produced in a given flow batch.
    /// </summary>
    public class FlowSnapshot
    {
        /// <summary>
        ///     Gets the time time spent processing from the incoming data source.
        /// </summary>
        public TimeSpan ProcessTime { get; set; }

        /// <summary>
        ///     Gets any errors encountered processing from the incoming data source.
        /// </summary>
        public List<Exception> Exceptions { get; set; }

        /// <summary>
        ///     Gets the warnings.
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        ///     Gets the total number of entities processed from the source.
        /// </summary>
        public long ProcessedCount { get; set; }

        /// <summary>
        ///     Gets the flow's source type if any.
        /// </summary>
        public FlowEntity SourceType { get; set; }

        /// <summary>
        ///     Gets the address id of the source. This would typically be a network address.
        /// </summary>
        public string SourceAddressId { get; set; }

        /// <summary>
        ///     Gets/sets the target address id the results were last saved to.
        /// </summary>
        public string TargetAddressId { get; set; }

        /// <summary>
        ///     Gets the target output entity type.
        /// </summary>
        public FlowEntity TargetType { get; set; }

        /// <summary>
        ///     Gets the batch from which this snapshot originated from.
        /// </summary>
        public FlowBatch Batch { get; set; }

        /// <summary>
        ///     Gets the errors that were encountered processing the incoming entities.
        /// </summary>
        public List<ErrorEvent> Errors { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowSnapshot()
        {
            this.Warnings = new List<string>();
            this.Exceptions = new List<Exception>();
            this.Errors = new List<ErrorEvent>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowSnapshot(
            FlowBatch flowBatch) : this()
        {
            this.Batch = flowBatch;
        }

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        public FlowSnapshot(
            FlowBatch flowBatch,
            FlowEntity sourceType, string sourceAddressId,
            FlowEntity targetType, string targetAddressId = null) : this()
        {
            Guard.ArgumentNotNull(flowBatch, nameof(flowBatch));
            Guard.ArgumentNotNull(sourceType, nameof(sourceType));
            Guard.ArgumentNotNull(targetType, nameof(targetType));

            Batch = flowBatch;
            SourceType = sourceType;
            SourceAddressId = sourceAddressId;
            TargetAddressId = targetAddressId;
            TargetType = targetType;
        }
    }

    /// <summary>
    ///     The result state from processing data from a particular incoming type to an outgoing type.
    /// </summary>
    /// <typeparam name="TEntityOut">The output type.</typeparam>
    public class FlowSnapShot<TEntityOut> : FlowSnapshot
    {
        /// <summary>
        ///     Creates a new flow file snapshot.
        /// </summary>
        public FlowSnapShot() : base()
        {
            this.Valid = new List<TEntityOut>();
            this.Invalid = new List<TEntityOut>();
            this.TargetType = new FlowEntity(typeof(TEntityOut));
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowSnapShot(
            FlowBatch flowBatch) : base(flowBatch)
        {
            this.Batch = flowBatch;
            this.TargetType = new FlowEntity(typeof(TEntityOut));
        }

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        public FlowSnapShot(
            FlowBatch flowBatch,
            FlowEntity sourceType, string sourceAddressId,
            FlowEntity targetType, string targetAddressId)
            : base(flowBatch, sourceType, sourceAddressId, targetType, targetAddressId)
        {
            this.Valid = new List<TEntityOut>();
            this.Invalid = new List<TEntityOut>();
        }

        /// <summary>
        ///     Gets the valid items created from a snapshot. This is the primary ouput.
        /// </summary>
        public List<TEntityOut> Valid { get; set; }

        /// <summary>
        ///     Gets the invalid items that were not processed in a flow snapshot.
        /// </summary>
        public List<TEntityOut> Invalid { get; set; }
    }
}