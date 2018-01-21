namespace FutureState.Flow.Core
{
    public class ProcessError<TEntityDto>
    {
        public ErrorEvent Error { get; set; }

        public TEntityDto Item { get; set; }
    }
}