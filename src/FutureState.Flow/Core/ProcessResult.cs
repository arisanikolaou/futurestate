using System;
using System.Collections.Generic;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Gets the result of processing data from an incoming data source.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        ///     Gets the time time spent processing from the incoming data source.
        /// </summary>
        public TimeSpan LoadTime { get; set; }

        /// <summary>
        ///     Gets any errors encountered processing from the incoming data source.
        /// </summary>
        public List<Exception> Errors { get; set; }

        /// <summary>
        ///     Gets the warnings.
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        ///     Gets the total number of entities processed from the source.
        /// </summary>
        public int ProcessedCount { get; set; }

        /// <summary>
        ///     Gets the job id or correlation id.
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        ///     Gets the batch id.
        /// </summary>
        public int BatchId { get; set; }
    }
}