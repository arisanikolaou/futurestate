using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Core;
using FutureState.Flow.Model;
using NLog;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Controls the flow of data from an incoming batch source to a downstream processor.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public abstract class FlowFileFlowFileBatchController<TIn,TOut> : IFlowFileBatchController
        where TOut : class, new()
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _outDirectory;
        private string _inDirectory;
        private readonly IReader<TIn> _reader;

        /// <summary>
        ///     Gets the controller name.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///     Gets/sets the unique flow id.
        /// </summary>
        public Guid FlowId { get; set; }


        protected FlowFileFlowFileBatchController(IReader<TIn> reader)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            OutDirectory = Environment.CurrentDirectory;
            InDirectory = Environment.CurrentDirectory;

            // assign name from type name by default
            ControllerName = GetType().Name;
            FlowId = SeqGuid.Create();

            _reader = reader;
        }

        public abstract Processor<TIn, TOut> GetProcessor();

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
                // determine if the file was processed by the given processor
                var processLogEntry = log.Entries.FirstOrDefault(
                    m => string.Equals(flowFile.FullName, m.FlowFileProcessed,
                        StringComparison.OrdinalIgnoreCase) 
                         && string.Equals(ControllerName, m.ControllerName, 
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


        public virtual ProcessResult Process(FileInfo flowFile, BatchProcess process)
        {
            try
            {
                // read the incoming batch of data
                IEnumerable<TIn> incomingData = Read(flowFile);

                // create the processor to batch process it
                var processor = GetProcessor();

                ProcessResult<TIn, TOut> result = processor.Process(incomingData, process);

                // save results to output directory
                if (!Directory.Exists(OutDirectory))
                    Directory.CreateDirectory(OutDirectory);

                var outputRepository = new ProcessResultRepository<ProcessResult>(OutDirectory);

                outputRepository.Save(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to process flow file {flowFile.Name} due to an unexpected error. Batch process is {process}.", ex);
            }
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