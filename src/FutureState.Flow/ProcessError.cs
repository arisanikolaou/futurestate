namespace FutureState.Flow
{
    /// <summary>
    ///     A generic error encountered processing a batch of entities.
    /// </summary>
    public class ProcessError
    {
        public ProcessError()
        {
            // required by serializer
        }

        public ProcessError(string type, string message, int processIndex)
        {
            Type = type;
            Message = message;
            ProcessIndex = processIndex;
        }

        public string Type { get; set; }

        public string Message { get; set; }

        public int ProcessIndex { get; set; }
    }
}