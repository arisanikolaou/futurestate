using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Core;
using NLog;

namespace FutureState.Flow.Flow
{
    public abstract class FlowFileBatchProcessor<TIn,TOut> : IBatchProcessor
        where TOut : class, new()
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _outDirectory;
        private string _inDirectory;
        private readonly IReader<TIn> _reader;

        public string Name { get; set; }

        protected FlowFileBatchProcessor(IReader<TIn> reader)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            OutDirectory = Environment.CurrentDirectory;
            InDirectory = Environment.CurrentDirectory;

            _reader = reader;
        }

        public abstract Processor<TIn, TOut> Configure();

        /// <summary>
        ///     Gets the flow files associated with the current directory.
        /// </summary>
        /// <returns></returns>
        public FileInfo GetNextFlowFile(FlowFileLog log)
        {
            if (!Directory.Exists(InDirectory))
                Directory.CreateDirectory(InDirectory);

            // this enumerate working folder
            var flowFiles = new DirectoryInfo(InDirectory)
                .GetFiles()
                .OrderBy(m => m.CreationTimeUtc);

            foreach (var flowFile in flowFiles)
            {
                // determine if the file was processed
                var processLogEntry = log.Entries.FirstOrDefault(
                    m => string.Equals(flowFile.FullName, m.BatchFilesProcessed,
                        StringComparison.OrdinalIgnoreCase));

                if (processLogEntry == null)
                    return flowFile;
            }

            return null;
        }

        protected virtual IEnumerable<TIn> Read(FileInfo flowFile)
        {
            return _reader.Read(flowFile.FullName);
        }


        public ProcessResult Process(FileInfo flowFile, BatchProcess process)
        {
            IEnumerable<TIn> incomingData = Read(flowFile);

            ProcessResult<TIn, TOut> result = Configure().Process(incomingData, process);

            if (!Directory.Exists(OutDirectory))
                Directory.CreateDirectory(OutDirectory);

            var outputRepository = new ProcessResultRepository<ProcessResult>(OutDirectory);

            outputRepository.Save(result);

            return result;
        }

        public string InDirectory
        {
            get => _inDirectory;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(InDirectory));

                _inDirectory = value;
            }
        }


        public string OutDirectory
        {
            get => _outDirectory;
            set
            {
                Guard.ArgumentNotNullOrEmptyOrWhiteSpace(value, nameof(OutDirectory));

                _outDirectory = value;
            }
        }
    }
}