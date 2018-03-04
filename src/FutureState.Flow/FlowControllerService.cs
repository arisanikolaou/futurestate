using FutureState.Flow.Controllers;
using FutureState.Flow.Data;
using FutureState.Flow.Model;
using NLog;
using System;
using System.IO;
using System.Timers;

namespace FutureState.Flow
{
    // query sources every n minutes and execute processor

    /// <summary>
    ///     Ensures that only unique batches of data sourced from a controller are processed every N minutes.
    /// </summary>
    /// <remarks>   
    ///     Uses a polling consumer pattern to query for source files.
    /// </remarks>
    public class FlowFileControllerService : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IFlowFileLogRepo _repo;
        private readonly Timer _timer;
        private readonly IFlowService _flowService;
        private volatile bool _isProcessing;
        private bool _started;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="logRepository">The repositoro to update transaction log details to.</param>
        /// <param name="flowFileController">The batch processor implementation.</param>
        public FlowFileControllerService(
            IFlowService flowService,
            IFlowFileLogRepo logRepository,
            IFlowFileController flowFileController)
        {
            Guard.ArgumentNotNull(flowService, nameof(flowService));
            Guard.ArgumentNotNull(logRepository, nameof(logRepository));
            Guard.ArgumentNotNull(flowFileController, nameof(flowFileController));

            _flowService = flowService;

            FlowFileController = flowFileController;
            Interval = TimeSpan.FromSeconds(30);

            _repo = logRepository;

            // the time to poll for incoming data
            _timer = new Timer();
            _timer.Elapsed += TryProcessNewFlowFiles;
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
        ///     Destructor.
        /// </summary>
        ~FlowFileControllerService()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Processing incoming data.
        /// </summary>
        private void TryProcessNewFlowFiles(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!_isProcessing)
                {
                    _isProcessing = true;

                    // get next available flow file from the source
                    FileInfo flowFile = TryGetNextFlowFile();

                    // flow file
                    if (flowFile == null)
                    {
                        if (_logger.IsDebugEnabled)
                            _logger.Debug(
                                $"No new flow file is available for the flow file controller {FlowFileController.ControllerName}.");

                        return;
                    }

                    // else
                    ProcessFlowFile(flowFile);
                }
                else
                {
                    if (_logger.IsTraceEnabled)
                        _logger.Trace("Still processing last flow file.");
                }
            }
            catch(Exception ex)
            {
                if(_logger.IsErrorEnabled)
                    _logger.Error(ex); // don't bubble up errors
            }
            finally
            {
                _isProcessing = false;
            }
        }

        FileInfo TryGetNextFlowFile()
        {
            // load transaction log db
            
            var flowFileLog = _repo.Get(
                FlowFileController.SourceEntityType,
                FlowFileController.Flow.Code);

            // get next available flow file from the source
            FileInfo flowFile = FlowFileController.GetNextFlowFile(flowFileLog);

            return flowFile;
        }

        private void ProcessFlowFile(FileInfo flowFile)
        {
            if (FlowFileController == null)
                throw new InvalidOperationException("Flow file controller not been configured or assigned.");


            try
            {
                // create a new flow batch - will create the flow entry if it does not exit
                var flowBatch = _flowService.GetNewFlowBatch(FlowFileController.Flow.Code);

                if (_logger.IsInfoEnabled)
                    _logger.Info(
                        $"New flow file {flowFile.Name} detected. Processing batch {flowBatch.BatchId} in flow {FlowFileController.Flow} by batch controller {FlowFileController.ControllerName}.");

                // run processor
                FlowSnapshot result = null;
                try
                {
                    result = FlowFileController.Process(flowFile, flowBatch);

                    if (result == null)
                        return; // no work to do
                }
                finally
                {
                    // update transaction log - always to ensure we are not re-processing the data
                    UpdateTransactionLog(flowFile.FullName, result);
                }

                if (_logger.IsInfoEnabled)
                    _logger.Info(
                        $"Flow file {flowFile.Name} processed in batch {flowBatch.BatchId} in flow {FlowFileController.Flow}.");

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Flow {FlowFileController.Flow} transaction log updated.");
            }
            catch (Exception ex)
            {
                var msg =
                    $"Failed to process flow file {flowFile.Name}. Flow file controller is {FlowFileController.GetType().Name}. Error: is {ex.Message}";

                throw new Exception(msg, ex);
            }
            finally
            {
                FlowFileProcessed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateTransactionLog(string sourceFilePath, FlowSnapshot result)
        {
            // update flow transaction log
            var flowFileLogEntry = new FlowFileLogEntry
            {
                AddressId = sourceFilePath,
                TargetAddressId = result.TargetAddressId, // the output file path
                BatchId = result.Batch.BatchId,
                DateLastUpdated = DateTime.UtcNow
            };

            _repo.Add(result.SourceType, result.Batch.Flow, flowFileLogEntry);
        }

        /// <summary>
        ///     Starts checking for batch data to process.
        /// </summary>
        public void Start()
        {
            if (_started)
                throw new InvalidOperationException("Service has already started.");


            if (Interval == default(TimeSpan))
            {
                int seconds = 15;

                // keep running
                if (_logger.IsErrorEnabled)
                    _logger.Error($"Invalid interval was configured. Defaulting to a polling interval of {seconds} seconds.");

                Interval = TimeSpan.FromSeconds(seconds);
            }

            // start polling
            _timer.Interval = Interval.TotalMilliseconds;
            _timer.Start();

            if (_logger.IsInfoEnabled)
                _logger.Info($@"Started polling for new flow files every {Interval.TotalSeconds} seconds.");


            _started = true;
        }

        /// <summary>
        ///     Stops checking for new data.
        /// </summary>
        public void Stop()
        {
            _timer?.Stop();

            if (_logger.IsInfoEnabled)
                _logger.Info($@"Stopped polling for new flow files.");

            _started = false; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposing)
                _timer?.Dispose();
        }
    }
}