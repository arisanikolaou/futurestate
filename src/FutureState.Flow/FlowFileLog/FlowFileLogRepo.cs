using FutureState.Flow.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FutureState.Flow.Data
{
    /// <summary>
    ///     Saves/loads the flow transasction log from an underlying data store.
    /// </summary>
    public interface IFlowFileLogRepo
    {
        /// <summary>
        ///     Upserts the data to the underlying data store.
        /// </summary>
        /// <param name="data"></param>
        void Save(FlowFileLog data);


        /// <summary>
        ///     Gets a flow file log for a given entity type.
        /// </summary>
        /// <param name="entityType">The entity type to process.</param>
        /// <param name="code">The flow code</param>
        /// <returns></returns>
        FlowFileLog Get(FlowEntity entityType,  string code);

        /// <summary>
        ///     Adds a new flow file log entry.
        /// </summary>
        /// <param name="entityType">The entity type being processed.</param>
        /// <param name="flow">The associated flow.</param>
        /// <param name="flowFileLogEntry"></param>
        void Add(
            FlowEntity entityType,
            FlowId flow,
            FlowFileLogEntry flowFileLogEntry);
    }

    /// <summary>
    ///     A repository for a given flow.
    /// </summary>
    public class FlowFileLogRepo : IFlowFileLogRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        static readonly object _syncLock = new object();

        private string _dataDir;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="dataDir">
        ///     The folder to read/write data to.
        /// </param>
        public FlowFileLogRepo(string dataDir = null)
        {
            _dataDir = dataDir ?? Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string DataDir
        {
            get => _dataDir;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(DataDir));

                _dataDir = value;
            }
        }

        /// <inheritdoc />
        public void Save(FlowFileLog log)
        {
            Guard.ArgumentNotNull(log, nameof(log));

            lock (_syncLock)
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

                // test to ensure we can convert the log
                var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());

                // update existing file
                var fileName =
                    $@"{DataDir}\flow-{log.FlowEntity.EntityTypeId}-log.json";

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

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Saving flow file transaction log to {fileName}.");

                File.WriteAllText(fileName, body);

                if (_logger.IsInfoEnabled)
                    _logger.Info($"Saved flow file transaction log to {fileName}.");
            }
        }

        /// <summary>
        ///     Gets the flow file log for the flow with the given id.
        /// </summary>
        /// <param name="entityType"> The flow entity type. </param>
        /// <param name="flowCode"> The flow id. </param>
        /// <returns>
        ///     A new or existing flow file log instance.
        /// </returns>
        public FlowFileLog Get(FlowEntity entityType, string flowCode)
        {
            lock (_syncLock)
            {
                var fileName =
                    $@"{DataDir}\flow-{entityType.EntityTypeId}-log.json";

                if (!File.Exists(fileName))
                    return new FlowFileLog(entityType, flowCode);

                // else deserialize the flow log
                var body = File.ReadAllText(fileName);

                return JsonConvert.DeserializeObject<FlowFileLog>(body);
            }
        }

        /// <summary>
        ///     Adds a new flow file log entry.
        /// </summary>
        /// <param name="source">The source entity type to use for processing.</param>
        /// <param name="flow">The flow id to process.</param>
        /// <param name="flowFileLogEntry">
        ///     The flow file log entry.
        /// </param>
        public void Add(
            FlowEntity source, 
            FlowId flow, 
            FlowFileLogEntry flowFileLogEntry)
        { 
            // load transaction log db
            FlowFileLog flowFileLog = Get(source, flow.Code);
            if (flowFileLog == null)
                throw new InvalidOperationException("Expected flow file log.");

            // update databasei
            if (flowFileLog.Entries == null)
                flowFileLog.Entries = new List<FlowFileLogEntry>();

            flowFileLog.Entries.Add(flowFileLogEntry);

            Save(flowFileLog);
        }
    }
}