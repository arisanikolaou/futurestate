using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace FutureState.Flow
{

    /// <summary>
    ///     Populates a given data source log file based on the files in a given directory
    ///     over a regular and configurable time interval.
    /// </summary>
    public class DirectoryBaseDataSourceProducer : IDisposable
    {
        private readonly DirectoryInfo[] _sourceDirectories;
        private readonly FlowEntity _flowEntity;
        private readonly DataSourceLogRepo _repository;
        private readonly Timer _timer;

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
        ///     Creates a new instance.
        /// </summary>
        /// <param name="flowEntity">The flow entity encoded in the directory.</param>
        /// <param name="sourceDirectories">The directories to poll for new files.</param>
        public DirectoryBaseDataSourceProducer(DirectoryInfo[] sourceDirectories, FlowEntity flowEntity, DataSourceLogRepo repository)
        {
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));

            _sourceDirectories = sourceDirectories;
            _flowEntity = flowEntity;
            _repository = repository;

            PollInterval = TimeSpan.FromSeconds(1);

            _timer = new Timer(PollInterval.TotalMilliseconds);
            _timer.Elapsed += _timer_Elapsed;
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

        public void Start()
        {
            if (this.PollInterval.TotalMilliseconds < 5)
                throw new InvalidOperationException("Poll interval cannot be less than a second.");

            _timer.Interval = this.PollInterval.TotalMilliseconds;
            _timer.Enabled = true;
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }


        public bool UpdateLog(DataSourceLog log)
        {
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
                    bool added = log.Add(file.FullName, file.LastAccessTimeUtc);

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

    /// <summary>
    ///     A repository for a data source log.
    /// </summary>
    public class DataSourceLogRepo
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets the base data directory.
        /// </summary>
        public string DataDir { get; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataSourceLogRepo(string dataDir)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(dataDir, nameof(dataDir));

            DataDir = dataDir;
        }

        // keep a log of the entities which errored out or were processed
        /// <summary>
        ///     Saves the result to the data dir.
        /// </summary>
        /// <param name="data">
        /// </param>
        public string Save(DataSourceLog data)
        {
            CreateDirIfNotExists();

            var i = 1;
            var fileName =
                $@"{DataDir}\datasource-{data.EntityType.EntityTypeId}.json";


            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving data source log to {fileName}.");


            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            if (File.Exists(fileName))
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug("Backing up old archive file.");

                // back up older file, don't delete
                string backFile = fileName + ".bak";
                if (File.Exists(backFile))
                    File.Delete(backFile);

                File.Move(fileName, backFile);
            }

            File.WriteAllText(fileName, body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved data source log to {fileName}.");

            return fileName;
        }


        /// <returns></returns>
        public DataSourceLog Get(FlowEntity flowEntity)
        {
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));

            var fileName = $@"{DataDir}\datasource-{flowEntity.EntityTypeId}.json";

            if (!File.Exists(fileName))
            {
                if (_logger.IsInfoEnabled)
                    _logger.Info($"File {fileName} could not be found.");

                return default(DataSourceLog);
            }

            if (_logger.IsInfoEnabled)
                _logger.Info($"Reading flow snapshot from from {fileName}.");

            var body = File.ReadAllText(fileName);

            var result = JsonConvert.DeserializeObject<DataSourceLog>(body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Read data source log {fileName}.");

            return result;
        }


        private void CreateDirIfNotExists()
        {
            if (!Directory.Exists(DataDir))
            {
                try
                {
                    Directory.CreateDirectory(DataDir);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {DataDir}.", ex);
                }
            }
        }
    }

    /// <summary>
    ///     List a set of consumable data source files.
    /// </summary>
    public class DataSourceLog
    {
        /// <summary>
        ///     Gets the log entries.
        /// </summary>
        public List<DataSourceLogEntry> Entries { get; set; }

        /// <summary>
        ///     Gets the entity type produced and consumable within the data source files.
        /// </summary>
        public FlowEntity EntityType { get; set; }

        /// <summary>
        ///     Gets the types of files included in the log e.g. csv or txt files. If null all files will be included in a given log.
        /// </summary>
        public string FileTypes { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataSourceLog()
        {
            Entries = new List<DataSourceLogEntry>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="entityType">The entity encoded in the data sources.</param>
        /// <param name="filePattern">The file pattern to use to scan for data source snapshot files.</param>
        public DataSourceLog(FlowEntity entityType, string filePattern) : this()
        {
            Guard.ArgumentNotNull(entityType, nameof(entityType));

            Entries = new List<DataSourceLogEntry>();
            EntityType = entityType;
            FileTypes = filePattern;
        }

        /// <summary>
        ///     Adds a unique log entry by address and last write time.
        /// </summary>
        public bool Add(string addressId, DateTime lastWriteTime)
        {
            bool containsEntry = Entries
                .Any(m => 
                string.Equals(addressId, m.AddressId, StringComparison.InvariantCultureIgnoreCase) &&
                m.DateLastUpdated == lastWriteTime);

            if (!containsEntry)
            {
                Entries.Add(new DataSourceLogEntry(addressId, lastWriteTime));

                return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     A data source log entry indicating a unique time and date that a file was produced.
    /// </summary>
    public class DataSourceLogEntry
    {
        /// <summary>
        ///     Gets the date the source file was produced.
        /// </summary>
        public DateTime DateLastUpdated { get; set; }

        /// <summary>
        ///     The address of the data source such as a full file path.
        /// </summary>
        public string AddressId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataSourceLogEntry()
        {

        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="addressId">The address of the data source e.g. the file path.</param>
        /// <param name="dateLastUpdated">The date the data source was last updated.</param>
        public DataSourceLogEntry(string addressId, DateTime dateLastUpdated)
        {
            this.AddressId = addressId;
            this.DateLastUpdated = dateLastUpdated;
        }
    }
}