using System;
using System.IO;
using Newtonsoft.Json;

namespace FutureState.Flow.Flow
{
    public interface IFlowFileLogRepository
    {
        void Save(FlowFileLog data);

        FlowFileLog Get(Guid processId);
    }

    public class FlowFileLogRepository : IFlowFileLogRepository
    {
        private string _workingFolder;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="workingFolder">
        ///     The folder to read/write data to.
        /// </param>
        public FlowFileLogRepository(string workingFolder = null)
        {
            WorkingFolder = workingFolder ?? Environment.CurrentDirectory;
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
            if (!Directory.Exists(WorkingFolder))
            {
                try
                {
                    Directory.CreateDirectory(WorkingFolder);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Can't create working folder {WorkingFolder}.", ex);
                }
            }

            var fileName =
                $@"{WorkingFolder}\{data.ProcessId}.json";

            if (File.Exists(fileName))
                File.Delete(fileName);

            var body = JsonConvert.SerializeObject(data, new JsonSerializerSettings());

            File.WriteAllText(fileName, body);
        }

        public FlowFileLog Get(Guid processId)
        {
            var fileName =
                $@"{WorkingFolder}\{processId}.json";

            if (!File.Exists(fileName))
            {
                return new FlowFileLog()
                {
                    ProcessId = processId
                };
            }
            // else

            var body = File.ReadAllText(fileName);

            return JsonConvert.DeserializeObject<FlowFileLog>(body);

        }
    }
}