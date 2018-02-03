using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     A container for a set of process flow state transaction records executed by a processor.
    /// </summary>
    public class FlowProcessState
    {
        public FlowProcessState()
        {
        }

        public FlowProcessState(string processId)
        {
            ProcessId = processId;
            Details = new List<FlowProcessStateCheckPoint>();
        }

        /// <summary>
        ///     Gets or sets the processor id that owns the state.
        /// </summary>
        public string ProcessId { get; set; }

        /// <summary>
        ///     Gets the process flow stae details.
        /// </summary>
        public List<FlowProcessStateCheckPoint> Details { get; set; }
    }
}