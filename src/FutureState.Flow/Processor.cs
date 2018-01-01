using FutureState.Flow.Data;
using FutureState.Specifications;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FutureState.Flow
{
    public class Processor : Processor<object, object>
    {
        public Processor(Func<object, object> process) : base(process)
        {
        }
    }

    /// <summary>
    ///     Processes data from one or more data sources via a lightweight event sourcing model.
    /// </summary>
    /// <typeparam name="TEntityOut">The entity type produced by the current instance.</typeparam>
    /// <typeparam name="TEntityIn">The incoming entity type expected from data sources queried in the incoming ports.</typeparam>
    public class Processor<TEntityOut, TEntityIn> : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly object _syncLock = new object();

        private Timer _timer;
        private readonly Func<TEntityIn, TEntityOut> _mapper;
        private bool _disposed = false;
        private ProcessConfigurationRepository _processConfigurationRepository;
        private ProcessStateRepository _processStateRepository;
        private PackageRepository<TEntityOut> _packageRepository;
        private readonly IEnumerable<ISpecification<TEntityOut>> _specsForEntity;
        private readonly List<ISpecification<IEnumerable<TEntityOut>>> _specsForCollection;

        /// <summary>
        ///     Gets the port source(s) data driving the current processor.
        /// </summary>
        public List<QuerySource<TEntityIn>> PortSources { get; set; }

        /// <summary>
        ///     Gets the processors configuration.
        /// </summary>
        public ProcessorConfiguration Configuration { get; set; }


        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="mapper">The mapper function to use.</param>
        /// <param name="configuration">The processor configuration to rely to.</param>
        /// <param name="specProvider">Provides rules to validate a single materialized entity.</param>
        /// <param name="specProviderForCollection">Provides rules to validate a collection of materialized entities.</param>
        /// <param name="querySources">The sources to read input data from.</param>
        public Processor(
            Func<TEntityIn, TEntityOut> mapper,
            ProcessorConfiguration configuration = null,
            IProvideSpecifications<TEntityOut> specProvider = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForCollection = null,
            params QuerySource<TEntityIn>[] querySources)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));


            Configuration = configuration ?? new ProcessorConfiguration($"processor.{typeof(TEntityOut).Name}");
            PortSources = new List<QuerySource<TEntityIn>>();
            if (querySources != null)
                PortSources.AddRange(querySources);

            // don't auto start in the contructor
            _processConfigurationRepository = new ProcessConfigurationRepository($"processor.{typeof(TEntityOut).Name}.config");
            _processStateRepository = new ProcessStateRepository(Configuration.FlowDirPath, typeof(TEntityOut));
            _packageRepository = new PackageRepository<TEntityOut>(Configuration.FlowDirPath);

            specProvider = specProvider ?? new SpecProvider<TEntityOut>();

            _specsForEntity = specProvider.GetSpecifications();
            _specsForCollection = specProviderForCollection?.GetSpecifications().ToList() ?? new List<ISpecification<IEnumerable<TEntityOut>>>();
        }

        public IEnumerable<TEntityOut> GetValidData() => _packageRepository.GetEntities<TEntityOut>();


        public IEnumerable<ProcessEntityError> GetInvalidData() => _packageRepository.Get<TEntityOut>().SelectMany(m => m.Invalid);

        /// <summary>
        ///     Starts the current instance.
        /// </summary>
        public void Start()
        {
            if (_logger.IsTraceEnabled)
                _logger.Trace("Starting processor.");

            this.Configuration = _processConfigurationRepository.Get();

            if (this.Configuration == null)
                throw new InvalidOperationException("Configuration has not been resolved.");

            if (this.Configuration.PollTime < 1)
                throw new InvalidOperationException("Configuration.PollTime must be a value greater than 0.");

            if (this.PortSources == null)
                throw new InvalidOperationException("PortSources cannot be null.");

            _timer?.Dispose();

            // keep polling for new data
            _timer = new Timer((o) =>
            {
                if (!_disposed)
                    this.Process();
            },
                this,
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(this.Configuration.PollTime));
        }

        /// <summary>
        ///     Stops polling for new data.
        /// </summary>
        public void Stop()
        {
            if (_logger.IsTraceEnabled)
                _logger.Trace("Stopping processor.");

            _timer?.Dispose();
        }

        /// <summary>
        ///     Processes (pull) data from the underlying query sources.
        /// </summary>
        /// <returns></returns>
        public virtual ProcessState Process()
        {
            if (PortSources == null)
                throw new InvalidOperationException("PortSources has not been assigned.");

            // todo: leaky abstraction
            _processStateRepository.BasePath = Configuration.FlowDirPath;
            _packageRepository.BasePath = Configuration.FlowDirPath;

            // load/create process state
            ProcessState state = _processStateRepository.Get();

            if (_disposed)
                return state;

            try
            {
                lock (_syncLock)
                    state = ProcessInner(state);
            }
            finally
            {
                // save flow state
                _processStateRepository.Save(state);
            }

            return state;
        }

        protected virtual ProcessState ProcessInner(ProcessState processState)
        {

            foreach (var portSource in PortSources)
            {
                Package<TEntityOut> package;

                ProcessFlowState flowState = null;

                try
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace($"Processing flows from {portSource.ToString()}.");

                    Guid lastCheckPoint = Guid.Empty;
                    var lastIndex = processState.Details.Count - 1;
                    if (lastIndex != -1)
                        lastCheckPoint = processState.Details[lastIndex].CheckPoint;

                    // query sources for new data
                    QueryResponse<TEntityIn> pSourcePackage = portSource
                        .Get(
                        Configuration.ProcessorId, // client id
                        lastCheckPoint, // sequence id
                        Configuration.PageSize);

                    flowState = new ProcessFlowState(pSourcePackage.Package.FlowId, pSourcePackage.CheckPointTo);

                    var outputData = new List<TEntityOut>();
                    var invalidData = new List<ProcessEntityError>();
                    var processErrors = new List<ProcessError>();

                    if (pSourcePackage?.Package?.Data == null)
                        throw new InvalidOperationException("Source data is null.");

                    int processIndex = 0;

                    // iterate through the list .. todo process in parallel?
                    foreach (var source in pSourcePackage.Package.Data)
                    {
                        processIndex++;

                        try
                        {
                            TEntityOut outEntity = _mapper(source);

                            // validate 
                            var mappingErrors = _specsForEntity.ToErrors(outEntity).ToList();

                            if (!mappingErrors.Any())
                            {
                                outputData.Add(outEntity);

                                OnEntityProcessed(outEntity);
                            }
                            else
                            {
                                invalidData.Add(new ProcessEntityError(outEntity, mappingErrors));
                                flowState.EntitiesInvalid++;
                            }

                            flowState.EntitiesProcessed++;
                        }
                        catch (Exception ex)
                        {
                            flowState.ErrorsCount++;

                            if (Configuration.FailOnError)
                            {
                                throw new Exception("Failed to process incoming entity due to an unexpected error.", ex);
                            }
                            else
                            {
                                if (_logger.IsErrorEnabled)
                                    _logger.Error(ex);

                                processErrors.Add(new ProcessError()
                                {
                                    Type = "Process",
                                    Message = ex.Message,
                                    ProcessIndex = processIndex,
                                });

                                // continue processing
                            }
                        }
                    }

                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Assembling output package.");

                    // validate collection
                    if (_specsForCollection != null)
                    {
                        var collectionErrors = _specsForCollection.ToErrors(outputData);
                        if (collectionErrors.Any())
                        {
                            throw new RuleException("Can't process output data as one of more rules were violated.", collectionErrors);
                        }
                    }

                    // save results attaching source package thread
                    package = new Package<TEntityOut>
                    {
                        FlowId = pSourcePackage.Package.FlowId,
                        Data = outputData,
                        Invalid = invalidData,
                        Errors = processErrors
                    };
                }
                catch (RuleException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (_logger.IsErrorEnabled)
                        _logger.Error(ex, "Failed to process package.");

                    throw;
                }

                // save to package store/repository
                try
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Saving output package.");

                    // save to local file system
                    this._packageRepository.Save(package);

                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Saved output package.");
                }
                catch (Exception ex)
                {
                    if (_logger.IsErrorEnabled)
                        _logger.Error(ex, "Failed to save output package.");

                    throw new Exception("Failed to save outgoing package due to an unexpected error.", ex);
                }

                // add when successful
                if (flowState != null)
                {
                    // set date completed
                    flowState.EndDate = DateTime.UtcNow;

                    processState.Details.Add(flowState);
                }
            }

            return processState;
        }

        /// <summary>
        ///     Called whenever an entity was successfully processed and is valid.
        /// </summary>
        /// <param name="validOutEntity">The valid output entity.</param>
        protected virtual void OnEntityProcessed(TEntityOut validOutEntity)
        {
            // reserve for use by base class
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // todo: dispose logic
                }

                _disposed = true;
            }
        }

        ~Processor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}