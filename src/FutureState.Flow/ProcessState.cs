using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     A container for a set of process flow state transaction records executed by a processor.
    /// </summary>
    public class ProcessState
    {
        public ProcessState()
        {
        }

        public ProcessState(string processId)
        {
            ProcessId = processId;
            Details = new List<ProcessFlowState>();
        }

        /// <summary>
        ///     Gets or sets the processor id that owns the state.
        /// </summary>
        public string ProcessId { get; set; }

        /// <summary>
        ///     Gets the process flow stae details.
        /// </summary>
        public List<ProcessFlowState> Details { get; set; }
    }
}