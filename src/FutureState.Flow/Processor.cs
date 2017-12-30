using FutureState.Flow.Data;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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

        private Timer _timer;
        private readonly Func<TEntityIn, TEntityOut> _mapper;
        private readonly object _syncLock = new object();
        private bool _disposed = false;
        private ProcessConfigurationRepository _processConfigurationRepository;
        private ProcessStateRepository _processStateRepository;
        private PackageRepository<TEntityOut> _packageRepository;

        /// <summary>
        ///     Gets the port source(s) data driving the current processor.
        /// </summary>
        public List<PortSource<TEntityIn>> PortSources { get; set; } = new List<PortSource<TEntityIn>>();

        /// <summary>
        ///     Gets the processors configuration.
        /// </summary>
        public ProcessorConfiguration Configuration { get; set; } = new ProcessorConfiguration();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Processor(Func<TEntityIn, TEntityOut> mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _processConfigurationRepository = new ProcessConfigurationRepository($"processor.{typeof(TEntityOut).Name}.config");
            _processStateRepository = new ProcessStateRepository(Environment.CurrentDirectory, typeof(TEntityOut));
            _packageRepository = new PackageRepository<TEntityOut>(Environment.CurrentDirectory);

            // don't auto start in the contructor
        }

        /// <summary>
        ///     Starts the current instance.
        /// </summary>
        public void Start()
        {
            if (_logger.IsTraceEnabled)
                _logger.Trace("Starting processor.");

            this.Configuration = _processConfigurationRepository.Get();

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
                throw new InvalidOperationException("PortSources is null");

            _processStateRepository.BasePath = Configuration.FlowDirPath;
            _packageRepository.BasePath = Configuration.FlowDirPath;

            ProcessState state = _processStateRepository.Get();

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

                var flowState = ProcessFlowState.Create();

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
                        Configuration.Id, // client id
                        lastCheckPoint, // sequence id
                        Configuration.WindowSize);

                    // assign flow id
                    flowState.FlowId = pSourcePackage.Package.FlowId;
                    flowState.CheckPoint = pSourcePackage.SequenceTo;

                    var outputData = new List<TEntityOut>();
                    var errors = new List<ErrorEvent>();

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

                            outputData.Add(outEntity);

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

                                errors.Add(new ErrorEvent()
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
                        Errors = errors
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
                state.Details.Add(flowState);
            }

            return state;
        }

        /// <summary>
        ///     Gets all entities processed by the current instance.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TEntityOut> Get()
        {
            lock (_syncLock)
            {
                var serializer = new JsonSerializer();

                if (Configuration == null)
                    throw new InvalidOperationException("No configuration has been assigned to the active instance.");

                foreach (var filePath in Directory.GetFiles(Configuration.FlowDirPath, $"data.{typeof(TEntityOut).Name}.*.json"))
                {
                    using (var file = File.OpenText(filePath))
                    {
                        var package = (Package<TEntityOut>)serializer.Deserialize(file, typeof(Package<TEntityOut>));

                        if (package.Data == null)
                            throw new InvalidOperationException("Failed to load package data.");

                        foreach (var item in package.Data)
                            yield return item;
                    }
                }

                yield break;
            }
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