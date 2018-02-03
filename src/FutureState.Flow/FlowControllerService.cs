using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using FutureState.Flow.Model;
using NLog;
using System;
using System.Timers;

namespace FutureState.Flow
{
    // query sources every n minutes and execute processor

    /// <summary>
    ///     Ensures that only unique batches of data sourced from a controller are processed every N minutes.
    /// </summary>
    public class FlowFileControllerService : IDisposable
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
        /// <param name="flowFileController">The batch processor implementation.</param>
        public FlowFileControllerService(
            IFlowFileLogRepository logRepository,
            IFlowFileController flowFileController)
        {
            Guard.ArgumentNotNull(logRepository, nameof(logRepository));
            Guard.ArgumentNotNull(flowFileController, nameof(flowFileController));

            FlowFileController = flowFileController;
            Interval = TimeSpan.FromSeconds(30);

            _logRepository = logRepository;

            // the time to poll for incoming data
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
        }

        /// <summary>
        ///     Gets the controller that gets/loads data from a source to a processor.
        /// </summary>
        public IFlowFileController FlowFileController { get; }

        /// <summary>
        ///     Gets how frequently to poll new data sourced from a batch controller.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        ///     Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Raised whenever a new flow file has been processed.
        /// </summary>
        public event EventHandler FlowFileProcessed;

        /// <summary>
        /// /   Destructor.
        /// </summary>
        ~FlowFileControllerService()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Processing incoming data.
        /// </summary>
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
            if (FlowFileController == null)
                throw new InvalidOperationException("Batch processor has not been configured or assigned.");

            // load transaction log db
            var flowFileLog = _logRepository.Get(FlowFileController.FlowId);

            // get next available flow file
            var flowFile = FlowFileController.GetNextFlowFile(flowFileLog);

            // flow file
            if (flowFile == null)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug($"No new data is available from the given flow controller {FlowFileController.ControllerName}.");

                return;
            }

            try
            {
                flowFileLog.BatchId++; // increment batch id

                // reate a new batch process
                var batchProcess = new BatchProcess
                {
                    FlowId = FlowFileController.FlowId,
                    BatchId = flowFileLog.BatchId
                };

                if (_logger.IsInfoEnabled)
                    _logger.Info(
                        $"New flow file {flowFile.Name} detected. Processing batch {batchProcess.BatchId} in flow {FlowFileController.FlowId} by batch controller {FlowFileController.ControllerName}.");

                // run processor
                var result = FlowFileController.Process(flowFile, batchProcess);

                if (result == null)
                    return; // no work to do

                // update flow transaction log
                var processLogEntry = new FlowFileLogEntry
                {
                    FlowFileProcessed = flowFile.FullName,
                    ControllerName = FlowFileController.ControllerName,
                    BatchId = flowFileLog.BatchId
                };

                flowFileLog.Entries.Add(processLogEntry);

                if (_logger.IsInfoEnabled)
                    _logger.Info(
                        $"Flow file {flowFile.Name} processed in batch {batchProcess.BatchId} in flow {FlowFileController.FlowId}.");

                // update database
                _logRepository.Save(flowFileLog);

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Flow {FlowFileController.FlowId} transaction log updated.");
            }
            catch (Exception ex)
            {
                var msg =
                    $"Failed to process flow file {flowFile.Name}. {ex.Message}. Batch controller is {FlowFileController.GetType().Name}.";

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