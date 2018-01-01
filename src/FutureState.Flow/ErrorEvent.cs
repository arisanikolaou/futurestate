using System.Collections.Generic;

namespace FutureState.Flow
{
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

    public class ProcessEntityError
    {
        public ProcessEntityError()
        {
            // required by serializer
        }

        public ProcessEntityError(object entity, List<Error> errors)
        {
            Entity = entity;
            Errors = errors;
        }

        public object Entity { get; set; }

        public List<Error> Errors { get; set; }
    }
}