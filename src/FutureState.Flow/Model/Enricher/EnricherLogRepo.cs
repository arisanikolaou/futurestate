using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace FutureState.Flow
{
    /// <summary>
    ///     Loads/start a given enrichment transaction log.
    /// </summary>
    public interface IEnricherLogRepo
    {
        /// <summary>
        ///     Gets the log of data sources used to enrich a given target entity set.
        /// </summary>
        /// <returns></returns>
        EnricherLog Get(FlowId flow, FlowEntity sourceEntityType);

        /// <summary>
        ///     Saves the log of data sources used to enrich a given target entity set.
        /// </summary>
        void Save(EnricherLog data);
    }

    /// <summary>
    ///     Loads/saves <see cref="EnricherLog"/> instances from the file system.
    /// </summary>
    public class EnricherLogRepository : IEnricherLogRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Gets/sets the directory to store/load data files from.
        /// </summary>
        public string DataDir { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnricherLogRepository()
        {
            this.DataDir = Environment.CurrentDirectory;
        }

        /// <summary>
        ///     Gets the enrichment log for a given target process type.
        /// </summary>
        public EnricherLog Get(FlowId flow, FlowEntity sourceEntityType)
        {
            Guard.ArgumentNotNull(flow, nameof(flow));
            Guard.ArgumentNotNull(sourceEntityType, nameof(sourceEntityType));

            // source id would be the name of a processor

            var fileName =
                $@"{DataDir}\{flow}-Enrichment-{sourceEntityType.EntityTypeId}.json";

            if (!File.Exists(fileName))
            {
                if (_logger.IsInfoEnabled)
                    _logger.Info($"File {fileName} could not be found.");

                return null;
            }

            var content = File.ReadAllText(fileName);

            var log = JsonConvert.DeserializeObject<EnricherLog>(
                content,
                new JsonSerializerSettings());

            return log;
        }

        /// <summary>
        ///     Saves the log to the active data dir.
        /// </summary>
        public void Save(EnricherLog log)
        {
            Guard.ArgumentNotNull(log, nameof(log));

            CreateDirIfNotExists();

            // source log
            var fileName =
                $@"{DataDir}\{log.Flow}-Enrichment-{log.SourceEntityType.EntityTypeId}.json";

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving enrichment log output to {fileName}.");

            // save the data in the data directory
            var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());

            if (File.Exists(fileName))
            {
                // back up older file, don't delete
                string backFile = fileName + ".bak";
                if (File.Exists(backFile))
                    File.Delete(backFile);

                File.Move(fileName, backFile);
            }

            // wrap in transaction
            File.WriteAllText(fileName, body);

            // 
            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved enrichment log output to {fileName}.");
        }

        private void CreateDirIfNotExists()
        {
            if (!Directory.Exists(DataDir))
                Directory.CreateDirectory(DataDir);
        }
    }
}