using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace FutureState.Flow.Tests.Aggregators
{
    // manages the enrichment process 
    public class EnrichmentController
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// </summary>
        public EnrichmentController()
        {
            SourceId = "UniqueId";
        }

        /// <summary>
        /// 
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        ///     Enriches the source data with data from parts.
        /// </summary>
        /// <typeparam name="TWhole">The whole data type to create</typeparam>
        /// <param name="flowId">The associated flow id.</param>
        /// <param name="source">The data source for 'whole' objects.</param>
        /// <param name="enrichers">The sources to enrich.</param>
        /// <returns></returns>
        public EnrichmentLog Enrich<TWhole>(
            Guid flowId,
            IEnumerable<TWhole> source,
            IEnumerable<IEnricher<TWhole>> enrichers)
        {
            // enrichment log
            var log = new EnrichmentLog
            {
                SourceId = SourceId,
                FlowId = flowId,
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
                    EnricherUniqueId = enricher.UniqueId
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