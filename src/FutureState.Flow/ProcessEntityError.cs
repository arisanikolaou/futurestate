using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     An error encountered processing a single entity.
    /// </summary>
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