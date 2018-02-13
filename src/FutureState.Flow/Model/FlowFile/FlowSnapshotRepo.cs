using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace FutureState.Flow.Data
{
    /// <summary>
    ///     Repository for a given process result.
    /// </summary>
    /// <typeparam name="TSnapShot">The process result type.</typeparam>
    public class FlowSnapshotRepo<TSnapShot> : IFlowSnapshotRepo<TSnapShot> where TSnapShot : FlowSnapshot
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _workingFolder;

        /// <summary>
        ///     Create a new results repository.
        /// </summary>
        public FlowSnapshotRepo(string dataDir = null)
        {
            DataDir = dataDir ?? Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string DataDir
        {
            get => _workingFolder;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(DataDir));

                _workingFolder = value;
            }
        }

        // keep a log of the entities which errored out or were processed
        /// <summary>
        ///     Saves the result to the data dir.
        /// </summary>
        /// <param name="data"></param>
        public void Save(TSnapShot data)
        {
            CreateDirIfNotExists();

            var i = 1;
            var fileName =
                $@"{DataDir}\{data.ProcessName}-{data.Batch.Flow.Code}-{data.Batch.BatchId}.json";

            while (File.Exists(fileName))
                fileName =
                    $@"{DataDir}\{data.ProcessName}-{data.Batch.Flow.Code}-{
                            data.Batch.BatchId
                        }-{i++}.json";

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving flow snapshot output to {fileName}.");

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved process output to {fileName}.");
        }

        public TSnapShot Get(string processName, string flowCode, long batchId)
        {
            var fileName = $@"{DataDir}\{processName}-{flowCode}-{batchId}.json";

            if (!File.Exists(fileName))
                return default(TSnapShot);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Reading process output from {fileName}.");

            var body = File.ReadAllText(fileName);

            var result = JsonConvert.DeserializeObject<TSnapShot>(body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Read process output from {fileName}.");

            return result;
        }

        public TSnapShot Get(string snapShotAddress)
        {
            // source id would be the name of a processor

            if (!File.Exists(snapShotAddress))
                return null;

            var content = File.ReadAllText(snapShotAddress);

            var flowSnapShot = JsonConvert.DeserializeObject<TSnapShot>(
                content,
                new JsonSerializerSettings());

            return flowSnapShot;
        }

        public void Save2(TSnapShot flowSnapShot)
        {
            Guard.ArgumentNotNull(flowSnapShot, nameof(flowSnapShot));

            CreateDirIfNotExists();

            // source log
            var fileName = flowSnapShot.Address; // this should be the unique address

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving flow output to {fileName}.");

            // save the data in the data directory
            var body = JsonConvert.SerializeObject(flowSnapShot, new JsonSerializerSettings());

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

            // wrap in transaction
            File.WriteAllText(fileName, body);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved flow file to {fileName}.");
        }

        private void CreateDirIfNotExists()
        {
            if (!Directory.Exists(DataDir))
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