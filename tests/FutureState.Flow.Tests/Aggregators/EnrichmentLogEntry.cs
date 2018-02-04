using System;

namespace FutureState.Flow.Tests.Aggregators
{
    public class EnrichmentLogEntry
    {
        public string EnricherUniqueId { get; set; }

        public DateTime DateCreated { get; set; }

        public long EntitiesEnriched { get; set; }
    }
}