using System;
using System.IO;
using Newtonsoft.Json;

namespace FutureState.Flow.Core
{
    public class ProcessResultRepository<T> : IProcessResultRepository<T> where T: ProcessResult
    {
        private readonly string _workingFolder;

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder => _workingFolder;

        /// <summary>
        ///     Create a new results repository.
        /// </summary>
        public ProcessResultRepository(string workingFolder)
        {
            this._workingFolder = workingFolder;
        }

        // keep a log of the entities which errored out or were processed
        public void Save(T data)
        {
            if (!Directory.Exists(WorkingFolder))
                try
                {
                    Directory.CreateDirectory(WorkingFolder);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {WorkingFolder}.", ex);
                }

            var i = 1;
            var fileName = $@"{_workingFolder}\{data.ProcessName}-{data.CorrelationId}-{data.BatchId}.json";
            while (File.Exists(fileName))
                fileName =
                    $@"{_workingFolder}\{data.ProcessName}-{data.CorrelationId}-{data.BatchId}-{i++}.json";

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);
        }

        public T Get(string processName, Guid correlationId, long batchId)
        {
            var fileName = $@"{_workingFolder}\{processName}-{correlationId}-{batchId}.json";

            if (File.Exists(fileName))
            {
                var body = File.ReadAllText(fileName);

                return JsonConvert.DeserializeObject<T>(body);
            }
            else
            {
                return default(T);
            }
        }
    }
}