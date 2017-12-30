using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class ProcessState
    {
        public Guid FlowId { get; set; }

        /// <summary>
        ///     Gets/set the last checkpoint.
        /// </summary>
        public Guid CheckPoint { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<ProcessFlowState> Details { get; set; }
    }

    public class ProcessFlowState
    {
        /// <summary>
        ///     Gets or sets the flow id.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        ///     Gets the starting checkpoint.
        /// </summary>
        public Guid CheckPoint { get; set; }

        /// <summary>
        ///     Gets the date the process started.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        ///     Gets the date the process completed.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        ///     Gets the total number of entities processed.
        /// </summary>
        public int EntitiesProcessed { get; set; }

        /// <summary>
        ///     Gets the total number of errors countered processing.
        /// </summary>
        public int ErrorsCount { get; set; }

        /// <summary>
        ///     Gets the machine that processed the data.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     Gets the user that processed the date.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        ///     Creates a default instance.
        /// </summary>
        public static ProcessFlowState Create()
        {
            return new ProcessFlowState()
            {
                StartDate = DateTime.UtcNow,
                Host = Environment.MachineName,
                User = Environment.UserName,
                CheckPoint = Guid.NewGuid()
            };
        }
    }
}