using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     A log of all the flow files processed for a particular flow processed for 
    ///     a particular flow entity type.
    /// </summary>
    /// <remarks>
    ///     Flow files are interchangeable as data file logs. Flow files can be used to data drive
    ///     other processors in a given flow. Flow file logs help decouple the system
    ///     from directory structure configuration.
    /// </remarks>
    public class FlowFileLog : DataFileLog
    {
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
        public FlowFileLog(FlowEntity flowEntity, string flowCode)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(flowCode, nameof(flowCode));
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));

            Entries = new List<FlowFileLogEntry>();
            EntityType = flowEntity;
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