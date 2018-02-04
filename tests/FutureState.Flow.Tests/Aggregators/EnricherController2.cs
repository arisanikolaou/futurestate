using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Castle.Components.DictionaryAdapter;
using FutureState.Flow.Data;
using FutureState.Specifications;

namespace FutureState.Flow.Tests.Aggregators
{
    public class EnricherController2
    {
        readonly ProcessorConfiguration<EntityIn, EntityOut> _processorConfiguration;
        readonly ProcessResultRepository<ProcessResult<EntityIn, EntityOut>> _repository;

        public EnricherController2()
        {
            this._processorConfiguration = new ProcessorConfiguration
                <EntityIn, EntityOut>(
                    new SpecProvider<EntityOut>(),
                    new SpecProvider<IEnumerable<EntityOut>>());

            // source directory default to current directory
            SourceDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            // source repository
            this._repository = new ProcessResultRepository<ProcessResult<EntityIn, EntityOut>>()
            {
                WorkingFolder = SourceDirectory.FullName
            };
        }

        public DirectoryInfo SourceDirectory { get; set; }

        public DirectoryInfo PartDirectory { get; set; }

        public void Initialize()
        {
            // initialize directories
            if (SourceDirectory == null)
                SourceDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        }

        public BatchProcess GetBatchProcess()
        {
            // load flow file
            var flowFileRepo = new FlowFileLogRepository();
            var flowFile = flowFileRepo.Get(Guid.Parse("a4840d97-6924-4f35-a7bf-c09ef5a0bb2c"));

            // save
            flowFileRepo.Save(flowFile);

            // load batch process
            var batchProcess = new BatchProcess()
            {
                FlowId = flowFile.FlowId,
                BatchId = flowFile.BatchId
            };

            return batchProcess;
        }

        public void Process(BatchProcess batchProcess)
        {
            foreach (var sourceFileInfo in SourceDirectory.GetFiles())
            {
                Process(batchProcess, sourceFileInfo);
            }
        }

        public void Process(BatchProcess batchProcess, FileInfo sourceFileInfo)
        {
            var enRepo = new EnrichmentLogRepository();

            // load by source id
            var log = enRepo.Get(sourceFileInfo.Name, batchProcess);

            var unProcessedEnrichers = new List<IEnricher<EntityOut>>();

            // aggregate list of enrichers
            foreach (var partDirectory in PartDirectory.GetFiles())
            {
                // this is the source items
                // load from sources
                var list = new List<EntityPart>
                {
                    new EntityPart() {Key = "Key", Name = "Name"}
                };
                // read from source

                var enricher = new Enricher<EntityPart, EntityOut>(() => list)
                {
                    UniqueId = partDirectory.Name
                };

                if (!log.GetHasBeenProcessed(batchProcess, enricher))
                {
                    // start processing
                    unProcessedEnrichers.Add(enricher);
                }
            }

            // load results to get the invalid items
            ProcessResult<EntityIn, EntityOut> result = _repository.Get(sourceFileInfo.FullName);

            //enricher.Enrich()
            var enrichmentController = new EnrichmentController();

            // enrich valid and invalid items
            enrichmentController
                .Enrich(batchProcess, result.Invalid, unProcessedEnrichers);

            // enrich valid and invalid items
            enrichmentController
                .Enrich(batchProcess, result.Output, unProcessedEnrichers);

            // save new output
            ProcessResults(result, batchProcess);
        }


        void ProcessResults(ProcessResult<EntityIn, EntityOut> result, BatchProcess batchProcess)
        {
            var outResult = new ProcessResult<EntityIn, EntityOut>()
            {
                Exceptions = new List<Exception>(),
                Errors = new List<ProcessError<EntityIn>>(),
                BatchProcess = batchProcess,
                Output = new List<EntityOut>(),
                Input = result.Input,
                Invalid = new List<EntityOut>()
            };

            var specs = _processorConfiguration.Rules.ToArray();
            var specsCollection = _processorConfiguration.CollectionRules.ToArray();

            var valid = new List<EntityOut>();
            var inValid = new List<EntityOut>();

            foreach (var source in new[] { result.Output, result.Invalid })
            {
                foreach (var entityOut in source)
                {
                    var errors = specs.ToErrors(entityOut).ToList();
                    if (!errors.Any())
                    {
                        valid.Add(entityOut);
                    }
                    else
                    {
                        // get all errors and add to error collection
                        foreach (var error in errors)
                        {
                            outResult.Errors.Add(new ProcessError<EntityIn>()
                            {
                                Error = new ErrorEvent() { Message = error.Message, Type = error.Type },
                                SourceItem = null,
                            });
                        }

                        inValid.Add(entityOut);
                    }
                }
            }

            // validate collection
            var collectionErrors = specsCollection.ToErrors(valid);

            if (!collectionErrors.Any())
            {
                result.Invalid = inValid;
                result.Output = valid;
            }
            else
            {
                // add all to invalid list
                inValid.AddRange(valid);

                result.Invalid = inValid;
            }

            // source repository
            var repo = new ProcessResultRepository<ProcessResult<EntityIn, EntityOut>>()
            {
                WorkingFolder = SourceDirectory.FullName
            };

            repo.Save(outResult);
        }

    }

    public class EntityIn
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Name1 { get; set; }
    }

    public class EntityPart : IEquatable<EntityOut>
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Name1 { get; set; }

        public bool Equals(EntityOut other)
        {
            return string.Equals(Key, other?.Key, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class EntityOut
    {
        public string Key { get; set; }


        public string Name { get; set; }
    }
}
