using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.Enrich
{
    // enrichers can/should work against 'invalid' entities as well as 'incomplete' entities
    // processed by a normal processor
    // enrichment should succeed or valid as a whole
    // should be able to work against a given process result

    /// <summary>
    ///     A log of all enrichments processed from a given source to multiple targets.
    /// </summary>
    public class EnrichmentLog
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnrichmentLog()
        {
            Logs = new List<EnrichmentLogEntry>();
            Exceptions = new List<Exception>();
        }

        // the processor results or source of the data

        /// <summary>
        ///     Gets the data source being enriched.
        /// </summary>
        public string TargetTypeId { get; set; }


        /// <summary>
        ///     Gets the log of all targets updated by data from the source.
        /// </summary>
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
        ///     Gets the date the enchrichment process finished if any. If null the flow batch was not completed.
        /// </summary>
        public DateTime? Completed { get; set; }

        /// <summary>
        ///     Gets whether or not the enricher has already been procesed.
        /// </summary>
        /// <returns></returns>
        public bool GetHasBeenProcessed(IEnricher enricher)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Logs != null)
                return Logs.Any(m =>
                    string.Equals(m.OutputTypeId, enricher.OutputTypeId, StringComparison.OrdinalIgnoreCase));

            return false;
        }
    }
}