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
        /// <param name="flowId">The id of the flow.</param>
        /// <returns></returns>
        FlowFileLog Get(Guid flowId);
    }

    public class FlowFileLogRepository : IFlowFileLogRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _workingFolder;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="workingFolder">
        ///     The folder to read/write data to.
        /// </param>
        public FlowFileLogRepository(string workingFolder = null)
        {
            _workingFolder = workingFolder ?? Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder
        {
            get => _workingFolder;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(WorkingFolder));

                _workingFolder = value;
            }
        }

        public void Save(FlowFileLog data)
        {
            Guard.ArgumentNotNull(data, nameof(data));

            if (!Directory.Exists(WorkingFolder))
                try
                {
                    Directory.CreateDirectory(WorkingFolder);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {WorkingFolder}.", ex);
                }

            var fileName =
                $@"{WorkingFolder}\FlowLog-{data.FlowId}.json";

            if (File.Exists(fileName))
                File.Delete(fileName);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving flow file transaction log to {fileName}.");

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved flow file transaction log to {fileName}.");
        }

        public FlowFileLog Get(Guid flowId)
        {
            var fileName =
                $@"{WorkingFolder}\FlowLog-{flowId}.json";

            if (!File.Exists(fileName))
                return new FlowFileLog(flowId);

            // else
            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<FlowFileLog>(body);
        }
    }
}