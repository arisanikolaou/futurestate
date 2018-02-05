using System;

namespace FutureState.Flow.Tests.Aggregators
{
    public interface IEnrichmentLogRepository
    {
        /// <summary>
        ///     Gets the log of data sources used to enrich a given target entity set.
        /// </summary>
        /// <param name="targetEntitySetId"></param>
        /// <param name="flowId">The flow id.</param>
        /// <returns></returns>
        EnrichmentLog Get(string targetEntitySetId, Guid flowId);

        /// <summary>
        ///     Saves the log of data sources used to enrich a given target entity set.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flowId"></param>
        void Save(EnrichmentLog data, Guid flowId);
    }
}