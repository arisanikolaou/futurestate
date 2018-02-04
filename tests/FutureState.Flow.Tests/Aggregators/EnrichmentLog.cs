using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.Tests.Aggregators
{
    // enrichers can/should work against 'invalid' entities as well as 'incomplete' entities
    // processed by a normal processor
    // enrichment should succeed or valid as a whole
    // should be able to work against a given process result

    // a log of all the files that have been content enriched
    public class EnrichmentLog
    {
        // the processor results or source of the data

        public string SourceId { get; set; }

        // a record of the file that was produced as the output
        public string OutputFlowFile { get; set; }

        // a record of the items that have been enriched
        public List<EnrichmentLogEntry> Logs { get; set; }

        /// <summary>
        ///     Gets the collection of errors encountered running the enchriment process.
        /// </summary>
        public List<Exception> Exceptions { get; set; }

        /// <summary>
        ///     Gets the process start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     Gets the date the enchrichment process finished if any.
        /// </summary>
        public DateTime? Completed { get; set; }

        /// <summary>
        ///     Gets the batch process id.
        /// </summary>
        public BatchProcess Batch { get; set; }

        /// <summary>
        ///     Gets whether or not the enricher has already been procesed.
        /// </summary>
        /// <returns></returns>
        public bool GetHasBeenProcessed(BatchProcess process, IEnricher enricher)
        {
            if (Batch == null)
                throw new InvalidOperationException();

            if (!Batch.Equals(process))
                return false;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Logs != null)
                return Logs.Any(m =>
                    string.Equals(m.EnricherUniqueId, enricher.UniqueId, StringComparison.OrdinalIgnoreCase));

            return false;
        }
    }
}