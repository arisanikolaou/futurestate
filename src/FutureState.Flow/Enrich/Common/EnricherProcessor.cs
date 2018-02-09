using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace FutureState.Flow.Enrich
{
    // process enrichers on a given taarget

    public class EnricherProcessor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// </summary>
        public EnricherProcessor()
        {
            SourceId = "UniqueId";
        }

        /// <summary>
        ///     Gets the unique id of the souce enriching a given target.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        ///     Enriches the source data with data from parts.
        /// </summary>
        /// <typeparam name="TWhole">The whole data type to enrich.</typeparam>
        /// <param name="source">The data source for 'whole' objects.</param>
        /// <param name="enrichers">The sources to enrich 'whole' target objects.</param>
        /// <returns></returns>
        public EnrichmentLog Enrich<TWhole>(
            IEnumerable<TWhole> source,
            IEnumerable<IEnricher<TWhole>> enrichers)
        {
            // enrichment log to record transactions
            var log = new EnrichmentLog
            {
                TargetTypeId = SourceId,
                StartTime = DateTime.UtcNow,
                Exceptions = new List<Exception>(),
                Logs = new List<EnrichmentLogEntry>()
            };

            // parallelize enrichment 
            foreach (var enricher in enrichers.AsParallel())
            {
                // record event
                var logEntry = new EnrichmentLogEntry
                {
                    DateCreated = DateTime.UtcNow,
                    OutputTypeId = enricher.UniqueId
                };

                // list of items to create
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var whole in source)
                {
                    try
                    {
                        // find the parts
                        var foundParts = enricher.Find(whole);

                        // ReSharper disable once SuspiciousTypeConversion.Global
                        foreach (var part in foundParts)
                        {
                            try
                            {
                                // enrich the whole from the part
                                enricher.Enrich(part, whole);

                                // increment entities enriched
                                logEntry.EntitiesEnriched++;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException(
                                    $"Failed to enrich {whole} from {part} due to an unexpected error.",
                                    ex);
                            }
                        }
                    }
                    catch (ApplicationException aex)
                    {
                        log.Exceptions.Add(aex);

                        throw;
                    }
                    catch (Exception ex)
                    {
                        log.Exceptions.Add(ex);

                        throw new Exception(
                            $"Failed to enrich whole {whole} from source {source} due to an unexpected error.", ex);
                    }
                }

                lock (this)
                {
                    // add log entry after completes
                    log.Logs.Add(logEntry);
                }
            }

            // the date/time completed
            log.Completed = DateTime.UtcNow;

            return log;
        }
    }
}