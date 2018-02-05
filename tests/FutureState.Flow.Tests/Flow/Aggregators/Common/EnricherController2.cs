using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Data;
using FutureState.Specifications;

namespace FutureState.Flow.Tests.Aggregators
{
    public class EnricherController2
    {
        private readonly EnrichmentLogRepository _enrichmentLogRepository;
        private readonly ProcessorConfiguration<Whole, Whole> _processorConfiguration;
        private readonly ProcessResultRepository<ProcessResult<Whole, Whole>> _repository;
        private readonly ISpecification<IEnumerable<Whole>>[] _entityCollection;
        private readonly ISpecification<Whole>[] _entityRules;

        //
        public EnricherController2() : this(
            new ProcessorConfiguration<Whole, Whole>(new SpecProvider<Whole>(),
                new SpecProvider<IEnumerable<Whole>>()))
        {
        }

        public EnricherController2(ProcessorConfiguration<Whole, Whole> config, string targetDirectory = null)
        {
            _processorConfiguration = config;

            // source targetDirectory default to current targetDirectory
            TargetDirectory = new DirectoryInfo(targetDirectory ?? Environment.CurrentDirectory);

            _enrichmentLogRepository = new EnrichmentLogRepository
            {
                WorkingFolder = Environment.CurrentDirectory
            };

            // source repository
            _repository = new ProcessResultRepository<ProcessResult<Whole, Whole>>
            {
                WorkingFolder = TargetDirectory.FullName
            };


            this._entityRules = _processorConfiguration.Rules.ToArray();
            this._entityCollection = _processorConfiguration.CollectionRules.ToArray();
        }

        public DirectoryInfo TargetDirectory { get; set; }

        public DirectoryInfo PartDirectory { get; set; }

        public void Initialize()
        {
            // initialize directories
            if (TargetDirectory == null)
                TargetDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        }

        public BatchProcess GetBatchProcess()
        {
            // load flow file
            var flowFileRepo = new FlowFileLogRepository();
            var flowFile = flowFileRepo.Get(Guid.Parse("a4840d97-6924-4f35-a7bf-c09ef5a0bb2c"));

            // save
            flowFileRepo.Save(flowFile);

            // load batch process
            var batchProcess = new BatchProcess
            {
                FlowId = flowFile.FlowId,
                BatchId = flowFile.BatchId
            };

            return batchProcess;
        }

        public void Process(Guid flowId)
        {
            foreach (var sourceFileInfo in TargetDirectory.GetFiles())
                Process(flowId, sourceFileInfo);
        }

        /// <summary>
        ///     Process unriched data from a given source file.
        /// </summary>
        protected void Process(Guid flowId, FileInfo sourceFileInfo)
        {
            // load by source id
            var log = _enrichmentLogRepository.Get(sourceFileInfo.Name, flowId);
            if (log == null)
                log = new EnrichmentLog(flowId);

            var unProcessedEnrichers = new List<IEnricher<Whole>>();

            // aggregate list of enrichers
            foreach (var partDirectory in PartDirectory?.GetFiles() ?? Enumerable.Empty<FileInfo>())
            {
                // this is the source items
                // load from sources
                var list = new List<Part>
                {
                    new Part {Key = "Key", FirstName = "Name"}
                };
                // read from source

                var enricher = new Enricher<Part, Whole>(() => list)
                {
                    UniqueId = partDirectory.Name
                };

                if (!log.GetHasBeenProcessed(flowId, enricher))
                    unProcessedEnrichers.Add(enricher);
            }

            // load results to get the invalid items
            var result = _repository.Get(sourceFileInfo.FullName);

            //enricher.Enrich()
            var enrichmentController = new EnrichmentController();

            // enrich valid and invalid items
            enrichmentController
                .Enrich(flowId, result.Invalid, unProcessedEnrichers);

            // enrich valid and invalid items
            enrichmentController
                .Enrich(flowId, result.Output, unProcessedEnrichers);

            // save new output
            ProcessResults(result);
        }

        private void ProcessResults(ProcessResult<Whole, Whole> result)
        {
            // output result
            var outResult = result.CreateNew();

            var valid = new List<Whole>();
            var inValid = new List<Whole>();
            var processErrors = new List<ProcessError<Whole>>();

            foreach (var source in new[] { result.Output, result.Invalid })
                foreach (var entityOut in source)
                {
                    // validate enity
                    var errors = _entityRules.ToErrors(entityOut).ToList();

                    if (!errors.Any())
                    {
                        // no error valid
                        valid.Add(entityOut);
                    }
                    else
                    {
                        // get all errors and add to error collection
                        foreach (var error in errors)
                            processErrors.Add(new ProcessError<Whole>
                            {
                                Error = new ErrorEvent { Message = error.Message, Type = error.Type },
                                SourceItem = null
                            });

                        inValid.Add(entityOut);
                    }
                }

            // validate collection
            var collectionErrors = _entityCollection.ToErrors(valid);

            if (!collectionErrors.Any())
            {
                result.Invalid = inValid;
                result.Output = valid;
            }
            else
            {
                // add all to invalid list
                inValid.AddRange(valid);

                // result.Output = 0
                result.Invalid = inValid;
                result.Output.Clear();
            }

            // reset process errors
            result.Errors = processErrors;

            // save resports
            _repository.Save(outResult);
        }

        protected virtual void Commit()
        {

        }
    }
}