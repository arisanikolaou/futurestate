using FutureState.Flow.Model;
using Newtonsoft.Json;
using NLog;
using System;
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
        /// <param name="entityType">The entity type to use as the data source.</param>
        /// <param name="entityType">The entity type to process.</param>
        /// <param name="code">The flow code</param>
        /// <returns></returns>
        FlowFileLog Get(FlowEntity entityType, FlowEntity processedEntityType, string code);


        /// <summary>
        ///     Adds a new flow file log entry.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="flow"></param>
        /// <param name="flowFileLogEntry"></param>
        void Add(
            FlowEntity source,
            FlowEntity target,
            FlowId flow,
            FlowFileLogEntry flowFileLogEntry);
    }

    /// <summary>
    ///     A repository for a given flow.
    /// </summary>
    public class FlowFileLogRepo : IFlowFileLogRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

        /// <summary>
        ///     Gets the flow files associated with the current directory.
        /// </summary>
        public FileInfo GetNextFlowFile(string sourceDirectory, FlowFileLog log)
        {
            if (!Directory.Exists(sourceDirectory))
                Directory.CreateDirectory(sourceDirectory);

            // this enumerate working folder
            var flowFiles = new DirectoryInfo(sourceDirectory)
                .GetFiles()
                .OrderBy(m => m.CreationTimeUtc)
                .ToList();

            if (flowFiles.Any())
            {
                foreach (var flowFile in flowFiles)
                {
                    // determine if the file was processed by the given processor
                    var processLogEntry = log.Entries.FirstOrDefault(
                        m => string.Equals(flowFile.FullName, m.AddressId,
                                 StringComparison.OrdinalIgnoreCase));

                    if (processLogEntry == null)
                        return flowFile;
                }
            }
            else
            {
                if (_logger.IsWarnEnabled)
                    _logger.Warn($"No files were discovered under {sourceDirectory}.");
            }

            return null;
        }

        /// <inheritdoc />
        public void Save(FlowFileLog log)
        {
            Guard.ArgumentNotNull(log, nameof(log));

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
                $@"{DataDir}\flow-{log.FlowCode}-log.json";

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

        /// <summary>
        ///     Gets the flow file log for the flow with the given id.
        /// </summary>
        /// <param name="entityType"> The flow entity type. </param>
        /// <param name="processedEntityType"> The flow entity type to process. </param>
        /// <param name="flowCode"> The flow id. </param>
        /// <returns>
        ///     A new or existing flow file log instance.
        /// </returns>
        public FlowFileLog Get(FlowEntity entityType, FlowEntity processedEntityType, string flowCode)
        {
            var fileName =
                $@"{DataDir}\flow-{flowCode}-{entityType.EntityTypeId}-{processedEntityType.EntityTypeId}-log.json";

            if (!File.Exists(fileName))
                return new FlowFileLog(entityType, flowCode, processedEntityType);

            // else deserialize the flow log
            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<FlowFileLog>(body);
        }

        /// <summary>
        ///     Adds a new flow file log entry.
        /// </summary>
        /// <param name="source">The source entity type to use for processing.</param>
        /// <param name="target">The target entity type to process.</param>
        /// <param name="flow">The flow id to process.</param>
        /// <param name="flowFileLogEntry">
        ///     The flow file log entry.
        /// </param>
        public void Add(
            FlowEntity source, 
            FlowEntity target, 
            FlowId flow, 
            FlowFileLogEntry flowFileLogEntry)
        { 
            // load transaction log db
            FlowFileLog flowFileLog = Get(source, target, flow.Code);
            if (flowFileLog == null)
                throw new InvalidOperationException("Expected flow file log.");

            // update database
            flowFileLog.Entries.Add(flowFileLogEntry);
            Save(flowFileLog);
        }
    }
}