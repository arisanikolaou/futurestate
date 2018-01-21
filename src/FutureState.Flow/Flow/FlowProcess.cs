using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A well known data flow.
    /// </summary>
    public class FlowProcess
    {
        public FlowProcess()
        {
        }

        public FlowProcess(Guid flowId, string displayName)
        {
            FlowId = flowId;
            DisplayName = displayName;
            DateCreated = DateTime.UtcNow;
        }

        public Guid FlowId { get; set; }

        public string DisplayName { get; set; }

        public DateTime DateCreated { get; set; }
    }
}