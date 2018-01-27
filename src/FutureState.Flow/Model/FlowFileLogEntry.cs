namespace FutureState.Flow.Flow
{
    public class FlowFileLogEntry
    {
        public string ConsumerId { get; set; }
        public int BatchId { get; set; }

        public string BatchFilesProcessed { get; set; }
    }
}