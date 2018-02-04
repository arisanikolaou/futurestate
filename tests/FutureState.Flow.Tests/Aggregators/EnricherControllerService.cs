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
    public class EnricherControllerService
    {
        readonly Timer _timer;

        public EnricherControllerService()
        {
            _timer = new Timer
            {
                Interval = 1000
            };

            _timer.Elapsed += _timer_Elapsed;
        }

        public DirectoryInfo SourceDirectory { get; set; }

        public DirectoryInfo PartDirectory { get; set; }

        public void Start()
        {
            _timer.Start();
        }

        public void Process()
        {
            _timer.Start();

            // initialize directories
            if (SourceDirectory == null)
                SourceDirectory = new DirectoryInfo(Environment.CurrentDirectory);

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

            // source repository
            var repo = new ProcessResultRepository<ProcessResult<EntityIn, EntityOut>>()
            {
                WorkingFolder = SourceDirectory.FullName
            };

            foreach (var sourceFileInfo in SourceDirectory.GetFiles())
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
                ProcessResult<EntityIn, EntityOut> result = repo.Get(sourceFileInfo.FullName);

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
        }

        void ProcessResults(ProcessResult<EntityIn, EntityOut> result, BatchProcess batchProcess)
        {
            var processorConfiguration = new ProcessorConfiguration
                <EntityIn, EntityOut>(
                    new SpecProvider<EntityOut>(),
                    new SpecProvider<IEnumerable<EntityOut>>());

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

                var specs = processorConfiguration.Rules.ToArray();
                var specsCollection = processorConfiguration.CollectionRules.ToArray();

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

        public void Stop()
        {
            _timer.Stop();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process();
        }
    }
}
