using System;
using System.Collections.Generic;

namespace FutureState.Flow.Core
{
    public class ProcessOperationResult
    {
        public TimeSpan LoadTime { get; set; }
        public List<Exception> Errors { get; set; }
        public List<string> Warnings { get; set; }
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