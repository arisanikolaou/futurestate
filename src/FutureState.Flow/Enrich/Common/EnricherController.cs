using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Data;
using FutureState.Specifications;

namespace FutureState.Flow.Enrich
{

    /// <summary>
    ///     Controlls how a set of enrichers are used to append/update content in a
    ///     given flow file (process result).
    /// </summary>
    /// <typeparam name="TTarget">The target data type to enrich.</typeparam>
    public class EnricherController<TTarget> where TTarget :  class, new()
    {
        private readonly EnricherLogRepository _logRepo;
        private readonly ISpecification<IEnumerable<TTarget>>[] _entityCollection;
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
        public EnricherController(ProcessorConfiguration<TTarget, TTarget> config, string workingFolder = null)
        {
            var processorConfiguration = config;

            // targetDirectory workingFolder default to current workingFolder
            WorkingFolder = new DirectoryInfo(workingFolder ?? Environment.CurrentDirectory);

            _logRepo = new EnricherLogRepository()
            {
                DataDirectory = WorkingFolder.FullName
            };


            this._entityRules = processorConfiguration.Rules.ToArray();
            this._entityCollection = processorConfiguration.CollectionRules.ToArray();
        }

        /// <summary>
        ///     Gets the directory to store the log of files used to enrich target
        ///     flow files.
        /// </summary>
        public DirectoryInfo WorkingFolder { get; set; }


        /// <summary>
        ///     Initializes the controller.
        /// </summary>
        public void Initialize()
        {
            // initialize directories
            if (WorkingFolder == null)
                WorkingFolder = new DirectoryInfo(Environment.CurrentDirectory);
        }

        /// <summary>
        ///     Process the targets with a given set of enrichers.
        /// </summary>
        public void Process<TIn>(
            Guid flowId, 
            IEnumerable<IEnricher<TTarget>> enrichers, 
            IEnumerable<EnrichmentTarget<TIn, TTarget>> targets)
        {
            Initialize();

            foreach (var target in targets)
                Process(flowId, enrichers, target);
        }

        /// <summary>
        ///     Process unriched data from a given targetDirectory file.
        /// </summary>
        protected void Process<TPart>(
            Guid flowId, 
            IEnumerable<IEnricher<TTarget>> enrichers, 
            EnrichmentTarget<TPart, TTarget> target)
        {
            // load by targetDirectory id
            var logRepository = _logRepo.Get(target.UniqueId);

            if (logRepository == null)
                logRepository = new EnrichmentLog() { TargetTypeId = target.UniqueId };

            var unProcessedEnrichers = new List<IEnricher<TTarget>>();

            // aggregate list of enrichers that haven't been processed for the target
            foreach (var enricher in enrichers)
                if (!logRepository.GetHasBeenProcessed(enricher))
                    unProcessedEnrichers.Add(enricher);

            // get results
            var processResult = target.GetProcessResult();

            // enrich valid and invalid items
            var enrichmentController = new EnricherProcessor();
            foreach (var source in new[] { processResult.Invalid , processResult.Output })
                enrichmentController
                    .Enrich(source, unProcessedEnrichers);

            // process and save new enriched file
            {
                var outResult = ProcessResults(processResult);

                // save new file
                // targetDirectory repository
                var resultRepo = new ProcessResultRepository<ProcessResult<TPart, TTarget>>()
                {
                    WorkingFolder = target.File.Directory.FullName
                };

                // save resports
                resultRepo.Save(outResult);
            }

            // update enricher log
            {
                foreach (var enricher in unProcessedEnrichers)
                    logRepository.Logs.Add(new EnrichmentLogEntry() { OutputTypeId = enricher.UniqueId, DateCreated = DateTime.UtcNow });

                // save log
                _logRepo.Save(logRepository);
            }
        }

        private ProcessResult<TIn,TTarget> ProcessResults<TIn>(ProcessResult<TIn, TTarget> result)
        {
            // output result
            var outResult = result.CreateNew();
            if (outResult.BatchProcess.BatchId == result.BatchProcess.BatchId)
                throw new InvalidOperationException();

            var valid = new List<TTarget>();
            var inValid = new List<TTarget>();

            // output errors
            var processErrors = new List<ProcessError<TIn>>();

            // enrich all valid and invalid entities from the source
            foreach (var source in new[] { result.Output, result.Invalid })
            {
                foreach (var entityOut in source)
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
                            processErrors.Add(new ProcessError<TIn>
                            {
                                Error = new ErrorEvent { Message = error.Message, Type = error.Type }
                            });
                        }

                        inValid.Add(entityOut);
                    }
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

            return outResult;
        }

        protected virtual void Commit()
        {

        }
    }
}