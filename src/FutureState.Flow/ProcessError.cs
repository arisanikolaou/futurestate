namespace FutureState.Flow
{
    public class ProcessError<TEntityDto>
    {
        public ErrorEvent Error { get; set; }

        public TEntityDto Item { get; set; }
    }
}