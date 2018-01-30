using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A well known data flow.
    /// </summary>
    public class FlowProcess
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowProcess()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="displayName"></param>
        public FlowProcess(Guid processId, string displayName)
        {
            FlowId = processId;
            DisplayName = displayName;
            DateCreated = DateTime.UtcNow;
        }

        public Guid FlowId { get; set; }

        public string DisplayName { get; set; }

        public DateTime DateCreated { get; set; }
    }
}