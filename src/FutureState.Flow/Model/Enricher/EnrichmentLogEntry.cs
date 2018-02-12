using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A record of what output/procesor type has been updated from a given source enricher.
    /// </summary>
    public class EnrichmentLogEntry
    {
        /// <summary>
        ///     Gets the source address id.
        /// </summary>
        public string SourceAddressId { get; set; }

        /// <summary>
        ///     Gets the target address that was produced/enriched.
        /// </summary>
        public string TargetAddressId { get; set; }

        /// <summary>
        ///     Gets the batch process that was created to generate the target snapshot.
        /// </summary>
        public FlowBatch Batch { get; set; }

        /// <summary>
        ///     Gets the unique id of the snapshot enriching a given target.
        /// </summary>
        public string OutputTypeId { get; set; }
        /// <summary>
        ///     Gets the date the log entry was created in utc.
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        ///     Gets the number of entities enriched.
        /// </summary>
        public long EntitiesEnriched { get; set; }

        /// <summary>
        ///     Gets the target entity type that was enriched from a given source.
        /// </summary>
        public FlowEntity TargetEntity { get; set; }

        /// <summary>
        ///     Gets the date the enrichment process completed.
        /// </summary>
        public DateTime? Completed { get; internal set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnrichmentLogEntry()
        {

        }
    }
}