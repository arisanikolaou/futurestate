using FutureState.Flow.Data;
using FutureState.Specifications;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Controlls how a set of enrichers are used to append/update content in a
    ///     given flow file (process result).
    /// </summary>
    /// <typeparam name="TTarget">The target data type to enrich.</typeparam>
    public class EnricherController<TTarget> where TTarget : class, new()
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly EnricherLogRepository _logRepo;
        private readonly ISpecification<IEnumerable<TTarget>>[] _entityCollection;
        private readonly Flow _flow;
        private readonly ISpecification<TTarget>[] _entityRules;

        /// <summary>
        ///     Gets a new instance.
        /// </summary>
        public EnricherController() : this(
            new ProcessorConfiguration<TTarget, TTarget>(new SpecProvider<TTarget>(),
                new SpecProvider<IEnumerable<TTarget>>()))
        {
        }

        /// <summary>
        ///     Gets a new instance using a given process configuration operating against a given working folder.
        /// </summary>
        public EnricherController(ProcessorConfiguration<TTarget, TTarget> config, string dataDirectory = null)
        {
            var processorConfiguration = config;

            // targetDirectory workingFolder default to current workingFolder
            DataDir = new DirectoryInfo(dataDirectory ?? Environment.CurrentDirectory);

            _logRepo = new EnricherLogRepository()
            {
                DataDir = DataDir.FullName
            };

            this._entityRules = processorConfiguration.Rules.ToArray();
            this._entityCollection = processorConfiguration.CollectionRules.ToArray();
            this._flow = new Flow("FlowABD");
        }

        /// <summary>
        ///     Gets the directory to store the log of files used to enrich target
        ///     flow files.
        /// </summary>
        public DirectoryInfo DataDir { get; set; }

        /// <summary>
        ///     Initializes the controller.
        /// </summary>
        public void Initialize()
        {
            // initialize directories
            if (DataDir == null)
                DataDir = new DirectoryInfo(Environment.CurrentDirectory);
        }

        /// <summary>
        ///     Process unriched data from a given targetDirectory file.
        /// </summary>
        public void Process(
            FlowBatch flowBatch,
            IEnumerable<IEnricher<TTarget>> enrichers,
            IEnrichmentTarget<TTarget> target)
        {
            foreach (var enricher in enrichers)
            {
                // load by targetDirectory id
                var logRepository = _logRepo.Get(_flow, enricher.SourceEntityType);

                if (logRepository == null) // todo replace with flow
                    logRepository = new EnrichmentLog(flowBatch.Flow, enricher.SourceEntityType);

                // enrichers
                var unProcessedEnrichers = new List<IEnricher<TTarget>>();

                // aggregate list of enrichers that haven't been processed for the target
                if (logRepository.GetHasBeenProcessed(enricher, target.AddressId))
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Target has already been updated.");

                    continue;
                }

                // enrich valid and invalid items
                var enrichmentController = new EnricherProcessor(_logRepo);
                enrichmentController
                        .Enrich(flowBatch, new[] { target }, unProcessedEnrichers);

                // get results to save
                var results = target.Get();

                // output result
                var outResult = new FlowSnapShot<TTarget>(
                    flowBatch,
                    enricher.SourceEntityType,
                    enricher.AddressId,
                    new FlowEntity(typeof(TTarget)),
                    target.AddressId)
                {
                    Batch = flowBatch,
                    Address = target.AddressId,
                    TargetType = new FlowEntity(typeof(TTarget)),
                    SourceType = enricher.SourceEntityType,
                    SourceAddressId = enricher.AddressId,
                };

                // process and save new enriched file
                ProcessSnapShot(outResult, results);

                // save new flow file
                // targetDirectory repository
                var resultRepo = new FlowSnapshotRepo<FlowSnapShot<TTarget>>()
                {
                    DataDir = this.DataDir.FullName
                };

                // save resports
                resultRepo.Save(outResult);
            }
        }

        private void ProcessSnapShot(FlowSnapShot<TTarget> outResult, IEnumerable<TTarget> results)
        {
            var valid = new List<TTarget>();
            var inValid = new List<TTarget>();

            // output errors
            var processErrors = new List<ErrorEvent>();

            foreach (var entityOut in results)
            {
                // validate enity
                var errors = _entityRules.ToErrors(entityOut).ToList();

                if (!errors.Any())
                {
                    // no error - valid
                    valid.Add(entityOut);
                }
                else
                {
                    // get all errors and add to error collection
                    foreach (var error in errors)
                    {
                        processErrors.Add(new ErrorEvent { Message = error.Message, Type = error.Type });
                    }

                    inValid.Add(entityOut);
                }
            }

            // validate collection
            var collectionErrors = _entityCollection.ToErrors(valid);

            if (!collectionErrors.Any())
            {
                outResult.Invalid = inValid;
                outResult.Output = valid;
            }
            else
            {
                // add all to invalid list
                inValid.AddRange(valid);

                // result.Output = 0
                outResult.Invalid = inValid;
                outResult.Output.Clear();
            }

            // reset process errors
            outResult.Errors = processErrors;
        }

        protected virtual void Commit()
        {
        }
    }
}