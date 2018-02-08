using System;

namespace FutureState.Flow.Enrich
{
    public interface IEnrichmentLogRepository
    {
        /// <summary>
        ///     Gets the log of data sources used to enrich a given target entity set.
        /// </summary>
        /// <returns></returns>
        EnrichmentLog Get(string sourceId);

        /// <summary>
        ///     Saves the log of data sources used to enrich a given target entity set.
        /// </summary>
        void Save(EnrichmentLog data);
    }
}