namespace FutureState.Flow.Tests.Aggregators
{
    public interface IEnrichmentLogRepository
    {
        EnrichmentLog Get(string sourceId, BatchProcess process);

        void Save(EnrichmentLog data, BatchProcess process);
    }
}