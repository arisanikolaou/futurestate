using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A well known data flow.
    /// </summary>
    public class ProcessFlow
    {
        public ProcessFlow()
        {
        }

        public ProcessFlow(Guid flowId, string displayName)
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