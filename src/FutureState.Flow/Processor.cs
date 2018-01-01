using FutureState.Flow.Data;
using FutureState.Specifications;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// <typeparam name="TEntityOut">The entity produced by the current instance.</typeparam>
    /// <typeparam name="TEntityIn">The incoming entity data source.</typeparam>
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
        private readonly IEnumerable<ISpecification<TEntityOut>> _specifications;

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
        public Processor(
            Func<TEntityIn, TEntityOut> mapper, 
            ProcessorConfiguration configuration = null, 
            IProvideSpecifications<TEntityOut> specProvider = null,
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
            _specifications = specProvider.GetSpecifications();
        }

        public IEnumerable<TEntityOut> Get() =>  _packageRepository.Get<TEntityOut>();

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

        public virtual ProcessState Process()
        {
            if (PortSources == null)
                throw new InvalidOperationException("PortSources has not been assigned.");

            // todo: leaky abstraction
            _processStateRepository.BasePath = Configuration.FlowDirPath;
            _packageRepository.BasePath = Configuration.FlowDirPath;

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

        protected virtual ProcessState ProcessInner(ProcessState state)
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
                    var lastIndex = state.Details.Count - 1;
                    if (lastIndex != -1)
                        lastCheckPoint = state.Details[lastIndex].CheckPoint;

                    var pSourcePackage = portSource
                        .Get(
                        Configuration.ProcessorId, // client id
                        lastCheckPoint, // sequence id
                        Configuration.WindowSize);

                    flowState = new ProcessFlowState(pSourcePackage.Package.FlowId, pSourcePackage.SequenceTo);

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
                            var mappingErrors = _specifications.ToErrors(outEntity).ToList();

                            if (!mappingErrors.Any())
                                outputData.Add(outEntity);
                            else
                                invalidData.Add(new ProcessEntityError(outEntity, mappingErrors));

                            flowState.EntitiesInvalid ++;
                            flowState.EntitiesProcessed ++;
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
                            }
                        }
                    }

                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Assembling output package.");

                    // save results attaching source package thread
                    package = new Package<TEntityOut>
                    {
                        FlowId = pSourcePackage.Package.FlowId,
                        Name = pSourcePackage.Package.Name,
                        Data = outputData,
                        Invalid = invalidData,
                        Errors = processErrors
                    };
                }
                catch (Exception ex)
                {
                    if (_logger.IsErrorEnabled)
                        _logger.Error(ex, "Failed to process package.");

                    throw;
                }

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

                    throw;
                }

                // add when successful
                if(flowState != null)
                    state.Details.Add(flowState);
            }

            return state;
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