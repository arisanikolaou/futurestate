using FutureState.Diagnostics;
using FutureState.Services;
using System;
using System.IO;
using System.Timers;

namespace FutureState.Flow
{

    /// <summary>
    ///     Populates a given data source log file based on the files in a given directory
    ///     over a regular and configurable time interval.
    /// </summary>
    public class DirectoryBasedDataSourceProducer : IAgent, IDisposable
    {
        private readonly DirectoryInfo[] _sourceDirectories;
        private readonly FlowEntity _flowEntity;
        private readonly DataSourceLogRepo _repository;
        private readonly Timer _timer;
        private bool _hasStarted;

        /// <summary>
        ///     Gets the flow entity.
        /// </summary>
        public FlowEntity FlowEntity { get { return _flowEntity; } }

        /// <summary>
        ///     Gets the source data directory.
        /// </summary>
        public DirectoryInfo[] SourceDirectories { get { return _sourceDirectories; } }

        /// <summary>
        ///     Gets the interval to use to check for new files in the given directory.
        /// </summary>
        public TimeSpan PollInterval { get; set; }

        /// <summary>
        ///     Gets whether or not that servuce has started.
        /// </summary>
        public bool HasStarted { get { return _hasStarted; } }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="flowEntity">The flow entity encoded in the directory.</param>
        /// <param name="sourceDirectories">The directories to poll for new files.</param>
        /// <param name="polltime">The number of seconds to check for new data files and refresh the target directory.</param>
        public DirectoryBasedDataSourceProducer(DirectoryInfo[] sourceDirectories, FlowEntity flowEntity, DataSourceLogRepo repository , int polltime = 1)
        {
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));
            Guard.ArgumentNotNull(repository, nameof(repository));
            Guard.ArgumentNotNull(sourceDirectories, nameof(sourceDirectories));

            _sourceDirectories = sourceDirectories;
            _flowEntity = flowEntity;
            _repository = repository;

            _timer = new Timer(PollInterval.TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;

            PollInterval = TimeSpan.FromSeconds(polltime);
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                var log = _repository.Get(_flowEntity);

                // try to update the log
                if (UpdateLog(log))
                    _repository.Save(log);
            }
        }

        /// <summary>
        ///     Starts the background service to poll for new files.
        /// </summary>
        public void Start()
        {
            if (this.PollInterval.TotalMilliseconds < 5)
                throw new InvalidOperationException("Poll interval cannot be less than a second.");

            _timer.Interval = this.PollInterval.TotalMilliseconds;
            _timer.Enabled = true;

            _hasStarted = true;
        }

        /// <summary>
        ///     Stops the background service polling for new files.
        /// </summary>
        public void Stop()
        {
            _timer.Enabled = false;
            _hasStarted = false;
        }


        /// <summary>
        ///     Updates a given flow log file to match the entries in the target 
        ///     directory.
        /// </summary>
        /// <param name="log">The log file to update.</param>
        /// <returns>True if the log was updated with new entries.</returns>
        public bool UpdateLog(DataFileLog log)
        {
            Guard.ArgumentNotNull(log, nameof(log));

            bool hasChanges = false;

            foreach (var directory in _sourceDirectories)
            {
                FileInfo[] files;

                if (string.IsNullOrWhiteSpace(log.FileTypes))
                    files = directory.GetFiles();
                else
                    files = directory.GetFiles(log.FileTypes);

                foreach (FileInfo file in files)
                {
                    bool added = _repository.Add(log, file.FullName, file.LastWriteTimeUtc);

                    if (added)
                        hasChanges = added;
                }
            }

            return hasChanges;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}