using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     Gets the result of processing data from an incoming data source.
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
        ///     Gets or sets the process name.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///     Gets the network address of the snapshot.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     Gets the flow's source type if any.
        /// </summary>
        public FlowEntity SourceType { get; set; }

        /// <summary>
        ///     Gets the address id of the source. This would typically be a network address.
        /// </summary>
        public string SourceAddressId { get;  set; }

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
        ///     Creates a new instance
        /// </summary>
        public FlowSnapshot(
            FlowBatch flowBatch, 
            FlowEntity sourceType, string sourceAddressId, 
            FlowEntity targetType, string targetAddressId) : this()
        {
            Guard.ArgumentNotNull(sourceType, nameof(sourceType));
            Guard.ArgumentNotNull(targetType, nameof(targetType));

            Batch = flowBatch;
            SourceType = sourceType;
            SourceAddressId = sourceAddressId;
            Address = targetAddressId;
            TargetType = targetType;

            this.ProcessName = $"{targetType.EntityTypeId}-Process";
        }
    }

    /// <summary>
    ///     The result state from processing data from a particular incoming type to an outgoing type.
    /// </summary>
    /// <typeparam name="TEntityOut">The output type.</typeparam>
    public class FlowSnapShot<TEntityOut> : FlowSnapshot
    {
        public FlowSnapShot() : base()
        {
            this.Output = new List<TEntityOut>();
            this.Invalid = new List<TEntityOut>();
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
            this.Output = new List<TEntityOut>();
            this.Invalid = new List<TEntityOut>();
        }


        /// <summary>
        ///     Gets the valid items created after processing. This is the primary ouput.
        /// </summary>
        public List<TEntityOut> Output { get; set; }

        /// <summary>
        ///     Gets the invalid items that were not processed.
        /// </summary>
        public List<TEntityOut> Invalid { get; set; }
    }
}