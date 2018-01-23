using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     An error encountered processing a single entity.
    /// </summary>
    public class FlowProcessError
    {
        public FlowProcessError()
        {
            // required by serializer
        }

        public FlowProcessError(object entity, List<Error> errors)
        {
            Entity = entity;
            Errors = errors;
        }

        public object Entity { get; set; }

        public List<Error> Errors { get; set; }
    }
}