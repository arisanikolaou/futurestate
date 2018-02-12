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
    public interface IFlowFileLogRepository
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
        /// <returns></returns>
        FlowFileLog Get(string flowCode);
    }

    /// <summary>
    ///     A repository for a given flow.
    /// </summary>
    public class FlowFileLogRepository : IFlowFileLogRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _dataDir;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="DataDirectory">
        ///     The folder to read/write data to.
        /// </param>
        public FlowFileLogRepository(string DataDirectory = null)
        {
            _dataDir = DataDirectory ?? Environment.CurrentDirectory;
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

        public void Save(FlowFileLog log)
        {
            Guard.ArgumentNotNull(log, nameof(log));

            if (!Directory.Exists(DataDir))
                try
                {
                    Directory.CreateDirectory(DataDir);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {DataDir}.", ex);
                }

            // test to ensure we can convert the log
            var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());

            // update existing file
            var fileName =
                $@"{DataDir}\FlowLog-{log.FlowCode}.json";

            if (File.Exists(fileName))
                File.Delete(fileName);

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
        /// </returns>
        public FlowFileLog Get(string flowCode)
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);

            var fileName =
                $@"{DataDir}\FlowLog-{flowCode}.json";

            if (!File.Exists(fileName))
                return new FlowFileLog(flowCode);

            // else
            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<FlowFileLog>(body);
        }
    }
}