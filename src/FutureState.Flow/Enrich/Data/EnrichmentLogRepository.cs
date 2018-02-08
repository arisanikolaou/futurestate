using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Enrich
{
    public class EnrichmentLogRepository : IEnrichmentLogRepository
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string WorkingFolder { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public EnrichmentLogRepository()
        {
            this.WorkingFolder = Environment.CurrentDirectory;
        }


        public EnrichmentLog Get(string sourceId)
        {
            // source id would be the name of a processor

            var fileName =
                $@"{WorkingFolder}\EnrichmentLog-{sourceId}.json";

            if (!File.Exists(fileName))
                return null;

            var content = File.ReadAllText(fileName);

            var log = JsonConvert.DeserializeObject<EnrichmentLog>(
                content, 
                new JsonSerializerSettings());

            return log;
        }

        public void Save(EnrichmentLog data)
        {
            CreateDirIfNotExists();

            // source log
            var fileName =
                $@"{WorkingFolder}\EnrichmentLog-{data.SourceId}.json";

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saving data enrichment log output to {fileName}.");

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

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

            if (_logger.IsInfoEnabled)
                _logger.Info($"Saved enrichment log output to {fileName}.");
        }

        private void CreateDirIfNotExists()
        {
            if (!Directory.Exists(WorkingFolder))
                Directory.CreateDirectory(WorkingFolder);
        }
    }
}