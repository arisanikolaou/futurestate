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
        private readonly EnrichmentLogRepository _logRepo;
        private readonly ISpecification<IEnumerable<Whole>>[] _entityCollection;
        private readonly ISpecification<Whole>[] _entityRules;

        //
        public EnricherController2() : this(
            new ProcessorConfiguration<Whole, Whole>(new SpecProvider<Whole>(),
                new SpecProvider<IEnumerable<Whole>>()))
        {
        }

        public EnricherController2(ProcessorConfiguration<Whole, Whole> config, string workingFolder = null)
        {
            var processorConfiguration = config;

            // targetDirectory workingFolder default to current workingFolder
            WorkingFolder = new DirectoryInfo(workingFolder ?? Environment.CurrentDirectory);

            _logRepo = new EnrichmentLogRepository()
            {
                WorkingFolder = WorkingFolder.FullName
            };


            this._entityRules = processorConfiguration.Rules.ToArray();
            this._entityCollection = processorConfiguration.CollectionRules.ToArray();
        }

        public DirectoryInfo WorkingFolder { get; set; }


        public void Initialize()
        {
            // initialize directories
            if (WorkingFolder == null)
                WorkingFolder = new DirectoryInfo(Environment.CurrentDirectory);
        }

        public BatchProcess GetBatchProcess()
        {
            // load flow file
            var flowFileRepo = new FlowFileLogRepository()
            {
                WorkingFolder = WorkingFolder.FullName
            };
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

        public void Process(Guid flowId, IEnumerable<IEnricher<Whole>> enrichers, DirectoryInfo targetDirectory)
        {
            foreach (var sourceFileInfo in targetDirectory.GetFiles())
                Process(flowId, enrichers, sourceFileInfo);
        }

        /// <summary>
        ///     Process unriched data from a given targetDirectory file.
        /// </summary>
        protected void Process(Guid flowId, IEnumerable<IEnricher<Whole>> enrichers, FileInfo sourceFileInfo)
        {
            // load by targetDirectory id
            var log = _logRepo.Get(sourceFileInfo.Name, flowId);
            if (log == null)
                log = new EnrichmentLog(flowId) {SourceId = sourceFileInfo.Name};

            var unProcessedEnrichers = new List<IEnricher<Whole>>();

            // aggregate list of enrichers
            foreach(var enricher in enrichers)
                if (!log.GetHasBeenProcessed(flowId, enricher))
                    unProcessedEnrichers.Add(enricher);

            // targetDirectory repository
            var resultRepo = new ProcessResultRepository<ProcessResult<Whole, Whole>>();

            // load results to get the invalid items
            ProcessResult<Whole, Whole> result = resultRepo.Get(sourceFileInfo.FullName);

            //enricher.Enrich()
            var enrichmentController = new EnrichmentController();

            // enrich valid and invalid items
            enrichmentController
                .Enrich(flowId, result.Invalid, unProcessedEnrichers);

            // enrich valid and invalid items
            enrichmentController
                .Enrich(flowId, result.Output, unProcessedEnrichers);

            // save new output
            ProcessResults(sourceFileInfo.Directory, result);

            // save log
            _logRepo.Save(log, flowId);
        }

        private void ProcessResults(DirectoryInfo targetFolder, ProcessResult<Whole, Whole> result)
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


            // targetDirectory repository
            var resultRepo = new ProcessResultRepository<ProcessResult<Whole, Whole>>()
            {
                WorkingFolder = targetFolder.FullName
            };

            // save resports
            resultRepo.Save(outResult);
        }

        protected virtual void Commit()
        {

        }
    }
}