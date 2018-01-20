namespace FutureState.Flow.Core
{
    public interface IProcessor
    {
        string ProcessorType { get; }

        ProcessOperationResult Process();
    }
}