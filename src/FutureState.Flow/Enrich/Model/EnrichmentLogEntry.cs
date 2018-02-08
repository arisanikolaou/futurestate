using System;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    public class EnrichmentLogEntry
    {
        /// <summary>
        ///     Gets the unique id of the snapshot enriching a given target.
        /// </summary>
        public string EnricherUniqueId { get; set; }
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