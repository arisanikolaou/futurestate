namespace FutureState.Flow
{
    public class ErrorEvent
    {
        public string Type { get; set; }

        public string Message { get; set; }

        public int ProcessIndex { get; set; }
    }
}