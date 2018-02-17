using FutureState.Flow.Model;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

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
        ///     Loads the load data given a flow id.
        /// </summary>
        /// <param name="flowCode">The unique flow code.</param>
        /// <returns>A new or existing log for the given flow.</returns>
        FlowFileLog Get(string flowCode);
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
        /// <param name="flowCode">The flow id.</param>
        /// <returns>
        ///     A new or existing flow file log instance.
        /// </returns>
        public FlowFileLog Get(string flowCode)
        {
            var fileName =
                $@"{DataDir}\flow-{flowCode}-log.json";

            if (!File.Exists(fileName))
                return new FlowFileLog(flowCode);

            // else deserialize the flow log
            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<FlowFileLog>(body);
        }
    }
}