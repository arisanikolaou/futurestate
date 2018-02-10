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
        ///     Creates a new instance.
        /// </summary>
        public EnricherProcessor()
        {
           
        }

        /// <summary>
        ///     Enriches the source data with data from parts.
        /// </summary>
        /// <typeparam name="TWhole">The whole data type to enrich.</typeparam>
        /// <param name="targets">The data source for 'whole' objects that will be enriched..</param>
        /// <param name="enrichers">The sources to enrich 'whole' target objects.</param>
        /// <returns></returns>
        public EnrichmentLog Enrich<TWhole>(
            IEnumerable<TWhole> targets,
            IEnumerable<IEnricher<TWhole>> enrichers)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug("Enriching targets.");

            // enrichment log to record transactions
            var db = new EnrichmentLog
            {
                TargetTypeId = typeof(TWhole).Name,
                StartTime = DateTime.UtcNow,
                Exceptions = new List<Exception>(),
                Logs = new List<EnrichmentLogEntry>()
            };

            // parallelize enrichment of the targets
            foreach (var enricher in enrichers.AsParallel())
            {
                // record event so that it is not duplicated
                var logEntry = new EnrichmentLogEntry
                {
                    DateCreated = DateTime.UtcNow,
                    OutputTypeId = enricher.OutputTypeId
                };


                // list of items to create
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var target in targets)
                {
                    if (_logger.IsDebugEnabled)
                        _logger.Debug($"Enriching target with enricher {enricher.OutputTypeId}.");


                    try
                    {
                        // find the parts that can enrich the whole
                        var foundParts = enricher.Find(target);

                        // ReSharper disable once SuspiciousTypeConversion.Global
                        foreach (var part in foundParts)
                        {
                            try
                            {
                                // enrich the whole from the part
                                enricher.Enrich(part, target);

                                // increment entities enriched
                                logEntry.EntitiesEnriched++;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException(
                                    $"Failed to enrich {target} from {part} due to an unexpected error.",
                                    ex);
                            }
                        }
                    }
                    catch (ApplicationException aex)
                    {
                        db.Exceptions.Add(aex);

                        throw;
                    }
                    catch (Exception ex)
                    {
                        db.Exceptions.Add(ex);

                        throw new Exception(
                            $"Failed to enrich whole {target} from source {targets} due to an unexpected error.", ex);
                    }
                }

                lock (this)
                {
                    // add log entry after completes
                    db.Logs.Add(logEntry);
                }
            }

            // the date/time completed
            db.Completed = DateTime.UtcNow;

            return db;
        }
    }
}