using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Flow.Model
{
    /// <summary>
    ///     A log of all data files processed by a flow controller.
    /// </summary>
    /// <remarks>
    ///     Flow files are interchangeable as data file logs. Flow files can be used to data drive
    ///     other processors in a given flow. Flow file logs help decouple the system
    ///     from directory structure configuration.
    /// </remarks>
    public class FlowFileLog 
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
        public FlowFileLog(FlowEntity entityType, string flowCode)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(flowCode, nameof(flowCode));
            Guard.ArgumentNotNull(entityType, nameof(entityType));

            Entries = new List<FlowFileLogEntry>();
            FlowEntity = entityType;
            FlowCode = flowCode;
        }

        /// <summary>
        ///     Gets/sets the code of the associated flow.
        /// </summary>
        [Required]
        public string FlowCode { get; set; }

        /// <summary>
        ///     Gets the record of the flow file that have been produced.
        /// </summary>
        public List<FlowFileLogEntry> Entries { get; set; }

        /// <summary>
        ///     Gets/set the flow entity that would be produced from the incoming data source.
        /// </summary>
        public FlowEntity FlowEntity { get; set; }
    }
}