using System;
using System.Timers;
using NLog;

namespace FutureState.Flow.Flow
{
    // query sources every n minutes and execute processor

    public class FlowFileProcessor : IDisposable
    {
        private readonly Timer _timer;
        private volatile bool _isProcessing;
        readonly object _syncLock = new object();
        private readonly IFlowFileLogRepository _logRepository;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public FlowFileProcessor(IFlowFileLogRepository logRepository, IBatchProcessor batchProcessor)
        {
            Guard.ArgumentNotNull(logRepository,nameof(logRepository));
            Guard.ArgumentNotNull(batchProcessor, nameof(batchProcessor));

            BatchProcessor = batchProcessor;
            Interval = TimeSpan.FromSeconds(30);

            _logRepository = logRepository;
            
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
        }

        ~FlowFileProcessor()
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
                    _isProcessing = true;
                }
            }
        }

        void BeginProcesFlowFiles()
        {
            if (this.BatchProcessor == null)
            {
                if(_logger.IsErrorEnabled)
                    _logger.Error("Batch processor has not been configured or assigned.");
                return;
            }

            // load transaction log db
            FlowFileLog flowFileLog = _logRepository.Get(this.ProcessId);

            var flowFile = BatchProcessor.GetNextFlowFile(flowFileLog);

            if(flowFile != null)
            {
                flowFileLog.BatchId++; // increment batch id

                // reate a new batch process
                var batchProcess = new BatchProcess()
                {
                    ProcessId = this.ProcessId,
                    BatchId = flowFileLog.BatchId
                };

                var processLogEntry = new FlowFileLogEntry()
                {
                    BatchId = flowFileLog.BatchId
                };

                // run processor
                ProcessResult result = this.BatchProcessor.Process(flowFile, batchProcess);

                if (result != null)
                {
                    flowFileLog.Entries.Add(processLogEntry);

                    // update database
                    _logRepository.Save(flowFileLog);
                }
            }

        }

        public IBatchProcessor BatchProcessor { get; }

        public Guid ProcessId { get; set; }

        public TimeSpan Interval { get; set; }

        public void Start()
        {
            if (Interval != default(TimeSpan))
                _timer.Interval = Interval.TotalMilliseconds;
            else
                _timer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }


        private void Dispose(bool disposing)
        {
            if(disposing)
                _timer?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
