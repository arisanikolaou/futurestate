using FutureState.Flow.Enrich;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow
{
    /// <summary>
    ///     A log of all enrichments processed from a given source to multiple targets.
    /// </summary>
    public class EnricherLog
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnricherLog()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="flow">The related flow.</param>
        /// <param name="sourceEntityType">The source entity type id.</param>
        public EnricherLog(FlowId flow, FlowEntity sourceEntityType)
        {
            Guard.ArgumentNotNull(flow, nameof(flow));
            Guard.ArgumentNotNull(sourceEntityType, nameof(sourceEntityType));

            Flow = flow;
            SourceEntityType = sourceEntityType;
            Logs = new List<EnrichmentLogEntry>();
            Exceptions = new List<Exception>();
        }

        /// <summary>
        ///     Gets the flow associated with the current instance.
        /// </summary>
        public FlowId Flow { get; set; }

        /// <summary>
        ///     Gets/sets the entity type being used to enrich a target type.
        /// </summary>
        public FlowEntity SourceEntityType { get; set; }

        /// <summary>
        ///     Gets the log of all targets updated by data from the source.
        /// </summary>
        public List<EnrichmentLogEntry> Logs { get; set; }

        /// <summary>
        ///     Gets the collection of errors encountered running the enchriment process.
        /// </summary>
        public List<Exception> Exceptions { get; set; }

        /// <summary>
        ///     Gets whether or not the enricher has already been procesed.
        /// </summary>
        /// <returns></returns>
        public bool GetHasBeenProcessed(IEnricher enricher, string addressId)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Logs != null)
                return Logs.Any(m =>
                    string.Equals(m.TargetAddressId, addressId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.SourceAddressId, addressId, StringComparison.OrdinalIgnoreCase));

            return false;
        }
    }
}