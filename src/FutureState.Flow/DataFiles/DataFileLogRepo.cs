using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FutureState.Flow
{
    // use json data source repository for now

    /// <summary>
    ///     A repository for a data source log. This repository stores the list of consumable files representing a given
    ///     entity to one or many consumers.
    /// </summary>
    public class DataSourceLogRepo
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        readonly object _syncLock = new object();

        /// <summary>
        ///     Gets the base data directory.
        /// </summary>
        public string DataDir { get; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataSourceLogRepo(string dataDir = null)
        {
            DataDir = dataDir ?? Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets the total number of files/data sources that represent a given flow entity.
        /// </summary>
        /// <param name="flowEntity">The flow entity to deliver.</param>
        /// <returns></returns>
        public int Count(FlowEntity flowEntity)
        {
            return GetEntries(flowEntity).Count();
        }

        /// <summary>
        ///     Saves the result to the data dir.
        /// </summary>
        /// <param name="data">
        ///     The log to save.
        /// </param>
        public string Save(DataFileLog data)
        {
            Guard.ArgumentNotNull(data, nameof(data));

            lock (_syncLock)
            {
                CreateDirIfNotExists();

                var fileName =
                    $@"{DataDir}\flow-{data.EntityType.EntityTypeId}-log.json";

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Saving data source log to {fileName}.");

                string body = JsonConvert
                    .SerializeObject(data, new JsonSerializerSettings());

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
        }


        /// <returns></returns>
        public DataFileLog Get(FlowEntity flowEntity)
        {
            Guard.ArgumentNotNull(flowEntity, nameof(flowEntity));

            lock (_syncLock)
            {
                var fileName = $@"{DataDir}\flow-{flowEntity.EntityTypeId}-log.json";

                if (!File.Exists(fileName))
                {
                    if (_logger.IsInfoEnabled)
                        _logger.Info($"File {fileName} could not be found.");

                    return default(DataFileLog);
                }

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Reading data source log from {fileName}.");

                var body = File.ReadAllText(fileName);

                var result = JsonConvert.DeserializeObject<DataFileLog>(body);

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Read data source log {fileName}.");

                return result;
            }
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

        /// <summary>
        ///     Gets all the entries for a given flow entity.
        /// </summary>
        public IEnumerable<DataFileLogEntry> GetEntries(FlowEntity flowEntity)
        {
            return Get(flowEntity).Entries;
        }

        /// <summary>
        ///     Gets whether a given data source file has been processed at any point of time.
        /// </summary>
        public bool Contains(FlowEntity flowEntity, string addressId)
        {
            var entries = GetEntries(flowEntity);
            
            return entries.Any(m => m.AddressId.Equals(addressId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Records a new available and accessible data source file in the log.
        /// </summary>
        public bool Add(DataFileLog log, string addressId, DateTime dateLastUpdated)
        {
            bool containsEntry;

            lock (_syncLock)
            {
                Guard.ArgumentNotNull(log, nameof(log));
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(addressId, nameof(addressId));

                containsEntry = log.Entries
                    .Any(m =>
                    string.Equals(addressId, m.AddressId, StringComparison.InvariantCultureIgnoreCase) &&
                    m.DateLastUpdated == dateLastUpdated);
            }

            if (!containsEntry)
            {
                log.Entries.Add(new DataFileLogEntry(addressId, dateLastUpdated));

                Save(log); // update log

                return true;
            }
            else
            {
                // already added
            }

            return false;
        }
    }
}