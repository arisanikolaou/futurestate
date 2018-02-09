using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using FutureState.Flow.Data;
using FutureState.Specifications;

namespace FutureState.Flow.Enrich
{

    // polling consumer to enriche a target data source periodically

    public class EnricherControllerService
    {
        readonly Timer _timer;
        private readonly FlowFileLogRepository _flowFileRepo;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnricherControllerService(FlowFileLogRepository flowFileRepo)
        {
            _timer = new Timer
            {
                Interval = 1000
            };

            _flowFileRepo = flowFileRepo;
            _timer.Elapsed += _timer_Elapsed;
        }

        public DirectoryInfo SourceDirectory { get; set; }

        public DirectoryInfo PartDirectory { get; set; }

        public Guid FlowId { get; set; } = Guid.Parse("a4840d97-6924-4f35-a7bf-c09ef5a0bb2c");

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
            var flowFile = _flowFileRepo.Get(FlowId);

            // save
            _flowFileRepo.Save(flowFile);

            // load batch process
            var batchProcess = new BatchProcess()
            {
                FlowId = flowFile.FlowId,
                BatchId = flowFile.BatchId
            };

            // source repository
            var repo = new ProcessResultRepository<ProcessResult<Part, Whole>>()
            {
                WorkingFolder = SourceDirectory.FullName
            };

            var enRepo = new EnricherLogRepository();

            // load by source id
            foreach (var sourceFileInfo in SourceDirectory.GetFiles())
            {
                string sourceId = Path.GetFileNameWithoutExtension(sourceFileInfo.Name);

                var log = enRepo.Get( sourceId);

                var unProcessedEnrichers = new List<IEnricher<Whole>>();

                // aggregate list of enrichers
                foreach (var partDirectory in PartDirectory.GetFiles())
                {                
                    // this is the source items
                    // load from sources
                    var list = new List<Part>
                    {
                        new Part() {Key = "Key", FirstName = "Name"}
                    };

                    // read from source
                    var enricher = new Enricher<Part, Whole>(() => list)
                    {
                        UniqueId = partDirectory.Name
                    };

                    if (!log.GetHasBeenProcessed( enricher))
                    {
                        // start processing
                        unProcessedEnrichers.Add(enricher);
                    }
                }

                // load results to get the invalid items
                ProcessResult<Part, Whole> result = repo.Get(sourceFileInfo.FullName);

                //enricher.Enrich()
                var enrichmentController = new EnricherProcessor();

                // enrich valid and invalid items
                enrichmentController
                    .Enrich(result.Invalid, unProcessedEnrichers);

                // enrich valid and invalid items
                enrichmentController
                    .Enrich(result.Output, unProcessedEnrichers);

                // save new output
                ProcessResults(result, batchProcess);
            }
        }

        void ProcessResults(ProcessResult<Part, Whole> result, BatchProcess batchProcess)
        {
            var processorConfiguration = new ProcessorConfiguration
                <Part, Whole>(
                    new SpecProvider<Whole>(),
                    new SpecProvider<IEnumerable<Whole>>());

            {
                var outResult = new ProcessResult<Part, Whole>()
                {
                    Exceptions = new List<Exception>(),
                    Errors = new List<ProcessError<Part>>(),
                    BatchProcess = batchProcess,
                    Output = new List<Whole>(),
                    Invalid = new List<Whole>()
                };

                var specs = processorConfiguration.Rules.ToArray();
                var specsCollection = processorConfiguration.CollectionRules.ToArray();

                var valid = new List<Whole>();
                var inValid = new List<Whole>();

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
                                outResult.Errors.Add(new ProcessError<Part>()
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
                var repo = new ProcessResultRepository<ProcessResult<Part, Whole>>()
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
