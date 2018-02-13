using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.Enrich
{
    // process enrichers on a given taarget

    public class EnricherProcessor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EnricherLogRepository _logRepository;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnricherProcessor(EnricherLogRepository logRepository)
        {
            Guard.ArgumentNotNull(logRepository, nameof(logRepository));

            _logRepository = logRepository;
        }

        /// <summary>
        ///     Enriches the source data with data from parts.
        /// </summary>
        /// <typeparam name="TTarget">The whole data type to enrich.</typeparam>
        /// <param name="targets">The data source for 'whole' objects that will be enriched..</param>
        /// <param name="enrichers">The sources to enrich 'whole' target objects.</param>
        /// <returns></returns>
        public void Enrich<TTarget>(
            FlowBatch flowBatch,
            IEnumerable<IEnrichmentTarget<TTarget>> targets,
            IEnumerable<IEnricher<TTarget>> enrichers)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug("Enriching targets.");

            // parallelize enrichment of the targets
            foreach (var enricher in enrichers.AsParallel())
            {
                // enrichment log to record transactions
                var db = new EnrichmentLog(flowBatch.Flow, enricher.SourceEntityType);

                // list of items to create
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var target in targets)
                {
                    // record event so that it is not duplicated
                    var logEntry = new EnrichmentLogEntry
                    {
                        DateCreated = DateTime.UtcNow,
                        Batch = flowBatch,
                        SourceAddressId = enricher.AddressId,
                        TargetAddressId = target.AddressId
                    };

                    if (_logger.IsDebugEnabled)
                        _logger.Debug($"Enriching target with enricher {enricher}.");

                    foreach (var entity in target.Get())
                    {
                        try
                        {
                            // find the parts that can enrich the whole
                            var foundParts = enricher.Find(entity);

                            // ReSharper disable once SuspiciousTypeConversion.Global
                            foreach (var part in foundParts)
                            {
                                try
                                {
                                    // enrich the whole from the part
                                    enricher.Enrich(part, entity);

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

                    // update transasction log
                    lock (this)
                    {
                        // the date/time completed
                        logEntry.Completed = DateTime.UtcNow;
                        // add log entry after completes
                        db.Logs.Add(logEntry);
                    }
                }

                this._logRepository.Save(db);
            }
        }
    }
}