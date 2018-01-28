using System;
using System.Timers;
using FutureState.Flow.Data;
using FutureState.Flow.Model;
using NLog;

namespace FutureState.Flow
{
    // query sources every n minutes and execute processor

    /// <summary>
    ///     Ensures that only unique batches of data sourced from a controller are processed every N minutes.
    /// </summary>
    public class FlowFileProcessorService : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IFlowFileLogRepository _logRepository;
        private readonly object _syncLock = new object();
        private readonly Timer _timer;
        private volatile bool _isProcessing;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="logRepository">The repositoro to update transaction log details to.</param>
        /// <param name="flowFileBatchController">The batch processor implementation.</param>
        public FlowFileProcessorService(IFlowFileLogRepository logRepository,
            IFlowFileBatchController flowFileBatchController)
        {
            Guard.ArgumentNotNull(logRepository, nameof(logRepository));
            Guard.ArgumentNotNull(flowFileBatchController, nameof(flowFileBatchController));

            FlowFileBatchController = flowFileBatchController;
            Interval = TimeSpan.FromSeconds(30);

            _logRepository = logRepository;

            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
        }

        /// <summary>
        ///     Gets the controller that gets/loads data from a source to a processor.
        /// </summary>
        public IFlowFileBatchController FlowFileBatchController { get; }

        /// <summary>
        ///     Gets how frequently to poll new data sourced from a batch controller.
        /// </summary>
        public TimeSpan Interval { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Raised whenever a new flow file has been processed.
        /// </summary>
        public event EventHandler FlowFileProcessed;

        ~FlowFileProcessorService()
        {
            Dispose(false);
        }


        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncLock)
            {
                try
                {
                    if (!_isProcessing)
                    {
                        _isProcessing = true;

                        // load the logRepository
                        BeginProcesFlowFiles();
                    }
                }
                finally
                {
                    _isProcessing = false;
                }
            }
        }

        private void BeginProcesFlowFiles()
        {
            if (FlowFileBatchController == null)
                throw new InvalidOperationException("Batch processor has not been configured or assigned.");

            // load transaction log db
            var flowFileLog = _logRepository.Get(FlowFileBatchController.FlowId);

            var flowFile = FlowFileBatchController.GetNextFlowFile(flowFileLog);

            if (flowFile == null)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug("No new data is available from the given flow controller.");

                return;
            }

            try
            {
                flowFileLog.BatchId++; // increment batch id

                // reate a new batch process
                var batchProcess = new BatchProcess
                {
                    FlowId = FlowFileBatchController.FlowId,
                    BatchId = flowFileLog.BatchId
                };

                if (_logger.IsInfoEnabled)
                    _logger.Info(
                        $"New flow file {flowFile.Name} detected. Processing batch {batchProcess.BatchId} in flow {FlowFileBatchController.FlowId}.");

                // run processor
                var result = FlowFileBatchController.Process(flowFile, batchProcess);

                if (result != null)
                {
                    // update entry
                    var processLogEntry = new FlowFileLogEntry
                    {
                        FlowFileProcessed = flowFile.FullName,
                        ControllerName = FlowFileBatchController.ControllerName,
                        BatchId = flowFileLog.BatchId
                    };

                    flowFileLog.Entries.Add(processLogEntry);

                    if (_logger.IsInfoEnabled)
                        _logger.Info(
                            $"Flow file {flowFile.Name} processed in batch {batchProcess.BatchId} in flow {FlowFileBatchController.FlowId}.");

                    // update database
                    _logRepository.Save(flowFileLog);

                    if (_logger.IsInfoEnabled)
                        _logger.Info($"Flow {FlowFileBatchController.FlowId} transaction log updated.");
                }
            }
            catch (Exception ex)
            {
                var msg =
                    $"Failed to process flow file {flowFile.Name}. {ex.Message}. Batch controller is {FlowFileBatchController.GetType().Name}.";

                if (_logger.IsErrorEnabled)
                    _logger.Error(ex, msg);
            }
            finally
            {
                FlowFileProcessed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Starts checking for batch data to process.
        /// </summary>
        public void Start()
        {
            if (Interval != default(TimeSpan))
                _timer.Interval = Interval.TotalMilliseconds;
            else
                _timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;

            _timer.Start();

            if (_logger.IsInfoEnabled)
                _logger.Info($@"Started polling for new flow files every {Interval.TotalSeconds} seconds.");
        }

        /// <summary>
        ///     Stops checking for new data.
        /// </summary>
        public void Stop()
        {
            _timer?.Stop();

            if (_logger.IsInfoEnabled)
                _logger.Info($@"Stopped polling for new flow files.");
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _timer?.Dispose();
        }
    }
}