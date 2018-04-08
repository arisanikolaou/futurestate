namespace FutureState.Flow
{
    public interface IProcessorConfiguration
    {
        string InDirectory { get; set; }

        string OutDirectory { get; set; }
    }
}