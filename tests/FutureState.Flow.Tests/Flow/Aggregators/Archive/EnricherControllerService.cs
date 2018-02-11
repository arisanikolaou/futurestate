using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using FutureState.Flow.Data;
using FutureState.Specifications;
using NLog;

namespace FutureState.Flow.Enrich
{

    // polling consumer to enriche a target data source periodically
    /// <summary>
    ///     A service to automatically process updates from a given source to a target from csv source files.
    /// </summary>
    public class EnricherControllerService<TPart, TWhole> : IEnricherControllerService , IDisposable
        where TPart : IEquatable<TWhole>, new()
        where TWhole : class, new()
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly Timer _timer;
        private readonly FlowFileLogRepository _flowFileRepo;
        private readonly ProcessorConfiguration<TPart, TWhole> _prcessorConfiguration;
        private readonly ISpecification<TWhole>[] _specs;
        private readonly ISpecification<IEnumerable<TWhole>>[] _specsForCollection;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnricherControllerService(
            FlowFileLogRepository flowFileRepo, 
            ProcessorConfiguration<TPart, TWhole> config = null,
            int pollTime = 0)
        {
            _timer = new Timer
            {
                Interval = pollTime < 0 ? 1000 : pollTime
            };

            _flowFileRepo = flowFileRepo;
            _timer.Elapsed += _timer_Elapsed;

            // configuration to validate and process results
            _prcessorConfiguration = config ?? new ProcessorConfiguration
                <TPart, TWhole>(
                    new SpecProvider<TWhole>(),
                    new SpecProvider<IEnumerable<TWhole>>());

            // refresh rules and spects
            this._specs = _prcessorConfiguration.Rules.ToArray();
            this._specsForCollection = _prcessorConfiguration.CollectionRules.ToArray();
        }

        public DirectoryInfo TargetDirectoy { get; set; }


        public DirectoryInfo PartDataDir { get; set; }

        /// <summary>
        ///     Gets thef flow associated with instance.
        /// </summary>
        public Guid FlowId { get; set; } = Guid.Parse("a4840d97-6924-4f35-a7bf-c09ef5a0bb2c");

        /// <summary>
        ///     Starts automatically processing data from the source to the target.
        /// </summary>
        public void Start()
        {
            _timer.Start();
        }

        BatchProcess GetNewBatchProcess()
        {
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

            return batchProcess;
        }

        /// <summary>
        ///     Gets all the enrichers that have not been applied to a given target.
        /// </summary>
        /// <param name="targetEntityId">Gets the target entity type being enriched.</param>
        /// <returns></returns>
        protected virtual IEnumerable<IEnricher<TWhole>> GetEnrichers(string targetEntityId)
        {
            var enRepo = new EnricherLogRepository();

            var log = enRepo.Get(targetEntityId);

            foreach (var partDataFile in PartDataDir.GetFiles())
            {
                // this is the source items
                // load from sources
                var list = new List<TPart>();

                var builder = new CsvEnricherBuilder<TPart, TWhole>()
                {
                    FilePath = partDataFile.FullName
                };

                var enricher = builder.Get();

                if (!log.GetHasBeenProcessed(enricher))
                {
                    yield return enricher;
                }
            }
        }

        // get the flow files to process
        protected virtual ProcessResult<TPart, TWhole> GetTargetToEnrich(string targetEntityInstanceId)
        {
            var repo = new ProcessResultRepository<ProcessResult<TPart, TWhole>>()
            {
                DataDir = TargetDirectoy.FullName
            };

            // load results to get the invalid items
            ProcessResult<TPart, TWhole> result = repo.Get(targetEntityInstanceId);

            return result;
        }

        // gets the data sets to enrich
        protected virtual IEnumerable<ProcessResult<TPart, TWhole>> GetTargetsToEnrich()
        {
            // initialize directories
            if (TargetDirectoy == null)
                TargetDirectoy = new DirectoryInfo(Environment.CurrentDirectory);

            var batchProcess = GetNewBatchProcess();

            // load by source id
            foreach (var sourceFileInfo in TargetDirectoy.GetFiles())
            {
                string sourceId = Path.GetFileNameWithoutExtension(sourceFileInfo.Name);

                // gets all the enrichers to process against
                var unProcessedEnrichers = GetEnrichers(sourceId);

                // load results to get the invalid items
                ProcessResult<TPart, TWhole> result = GetTargetToEnrich(sourceFileInfo.FullName);

                yield return result;
            }
        }

        /// <summary>
        ///     Processes data from the source to the target.
        /// </summary>
        public void Process()
        {
            _timer.Start();

            var targetsToEnrich = GetTargetsToEnrich();

            // load by source id
            foreach (var result in targetsToEnrich)
            {
                // gets all the enrichers to process against
                var unProcessedEnrichers = GetEnrichers(result.ToString());

                //enricher.Enrich()
                var enrichmentController = new EnricherProcessor();

                // enrich valid and invalid items
                enrichmentController
                    .Enrich(result.Invalid, unProcessedEnrichers);

                // enrich valid and invalid items
                enrichmentController
                    .Enrich(result.Output, unProcessedEnrichers);

                // save new output
                ProcessResults(result);
            }
        }

        void ProcessResults(ProcessResult<TPart, TWhole> result)
        {
            // get new batch process
            var batchProcess = GetNewBatchProcess();

            var outResult = new ProcessResult<TPart, TWhole>()
            {
                Exceptions = new List<Exception>(),
                Errors = new List<ProcessError<TPart>>(),
                BatchProcess = batchProcess,
                Output = new List<TWhole>(),
                Invalid = new List<TWhole>()
            };

            var valid = new List<TWhole>();
            var inValid = new List<TWhole>();

            foreach (var source in new[] { result.Output, result.Invalid })
            {
                foreach (var entityOut in source)
                {
                    var errors = _specs.ToErrors(entityOut).ToList();
                    if (!errors.Any())
                    {
                        valid.Add(entityOut);
                    }
                    else
                    {
                        // get all errors and add to error collection
                        foreach (var error in errors)
                        {
                            outResult.Errors.Add(new ProcessError<TPart>()
                            {
                                Error = new ErrorEvent() {
                                    Message = error.Message,
                                    Type = error.Type },
                            });
                        }

                        inValid.Add(entityOut);
                    }
                }
            }

            // validate collection
            var collectionErrors = _specsForCollection.ToErrors(valid);

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

            // source repository save results
            var repo = new ProcessResultRepository<ProcessResult<TPart, TWhole>>()
            {
                DataDir = TargetDirectoy.FullName
            };

            repo.Save(outResult);
        }

        /// <summary>
        ///     Stops applying updates.
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
                Process();
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
