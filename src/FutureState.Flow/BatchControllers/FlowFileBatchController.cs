﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FutureState.Flow.Data;
using FutureState.Flow.Model;
using NLog;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Controls the flow of data from an incoming batch source to a downstream processor.
    /// </summary>
    /// <typeparam name="TIn">The incoming data type to </typeparam>
    /// <typeparam name="TOut"></typeparam>
    public class FlowFileFlowFileBatchController<TIn, TOut> : IFlowFileBatchController
        where TOut : class, new()
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Func<IFlowFileBatchController, Processor<TIn, TOut>> _getProcessor;
        private readonly IReader<TIn> _reader;
        private string _inDirectory;

        private string _outDirectory;

        /// <summary>
        /// </summary>
        /// <param name="reader">The reader to read incoming results from.</param>
        /// <param name="getProcessor">Function to create a new procesor.</param>
        /// <param name="config">Processor configuration settings.</param>
        public FlowFileFlowFileBatchController(
            IReader<TIn> reader,
            Func<IFlowFileBatchController, Processor<TIn, TOut>> getProcessor = null,
            ProcessorConfiguration<TIn, TOut> config = null)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            _getProcessor = getProcessor;
            if (_getProcessor == null)
                _getProcessor = controller => throw new NotImplementedException();

            Config = config ?? new ProcessorConfiguration<TIn, TOut>();

            OutDirectory = Environment.CurrentDirectory;
            InDirectory = Environment.CurrentDirectory;

            // assign name from type name by default
            ControllerName = $"{GetType().Name.Replace("`2", "")}-{typeof(TIn).Name}-{typeof(TOut).Name}";
            //create default flow it
            FlowId = SeqGuid.Create();

            _reader = reader;
        }


        /// <summary>
        ///     Gets the configuration to use to setup of a processor.
        /// </summary>
        public ProcessorConfiguration<TIn, TOut> Config { get; }

        /// <summary>
        ///     Gets the controller name.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///     Gets/sets the unique flow id.
        /// </summary>
        public Guid FlowId { get; set; }

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
                .OrderBy(m => m.CreationTimeUtc)
                .ToList();

            if (flowFiles.Any())
            {
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
            }
            else
            {
                if(_logger.IsWarnEnabled)
                    _logger.Warn($"No files were discovered under {InDirectory}.");
            }

            return null;
        }

        public virtual ProcessResult Process(FileInfo flowFile, BatchProcess process)
        {
            try
            {
                // read the incoming batch of data
                IEnumerable<TIn> incomingData = Read(flowFile);

                // create the processor to batch process it
                Processor<TIn, TOut> processor = GetProcessor();

                var result = processor.Process(incomingData, process);

                // save results to output directory
                if (!Directory.Exists(OutDirectory))
                    Directory.CreateDirectory(OutDirectory);

                var outputRepository = new ProcessResultRepository<ProcessResult>(OutDirectory);

                outputRepository.Save(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to process flow file {flowFile.Name} due to an unexpected error. Batch process is {process}.",
                    ex);
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

        public virtual Processor<TIn, TOut> GetProcessor()
        {
            return _getProcessor(this);
        }

        protected virtual IEnumerable<TIn> Read(FileInfo flowFile)
        {
            return _reader.Read(flowFile.FullName);
        }
    }
}