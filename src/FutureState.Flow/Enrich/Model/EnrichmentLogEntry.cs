using System;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     A record of what output/procesor type has been updated from a given source enricher.
    /// </summary>
    public class EnrichmentLogEntry
    {
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
    }
}