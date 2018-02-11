namespace FutureState.Flow.Enrich
{
    public interface IEnricherControllerService
    {
        void Process();
        void Start();
        void Stop();
    }
}