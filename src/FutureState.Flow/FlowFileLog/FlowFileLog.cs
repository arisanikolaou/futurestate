using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     A log of all the flow files processed for a particular flow processed for 
    ///     a particular flow entity type.
    /// </summary>
    public class FlowFileLog : DataFileLog
    {

        /// <summary>
        ///     Gets the type of entity that was produced.
        /// </summary>
        public FlowEntity TargetEntityType { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowFileLog(FlowEntity flowEntity, string flowCode, FlowEntity processedEntityType)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(flowCode, nameof(flowCode));
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));
            Guard.ArgumentNotNull(processedEntityType, nameof(processedEntityType));

            Entries = new List<FlowFileLogEntry>();
            EntityType = flowEntity;
            TargetEntityType = processedEntityType;
            FlowCode = flowCode;
        }

        /// <summary>
        ///     Gets/sets the flow code to use.
        /// </summary>
        [Required]
        public string FlowCode { get; set; }

        /// <summary>
        ///     Gets the flow file log entries to record 
        ///     which flow files were process againts a flow.
        /// </summary>
        public new List<FlowFileLogEntry> Entries { get; set; }
    }
}