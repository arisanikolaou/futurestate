using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmitMapper;
using FutureState.Specifications;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes a single type in income entity data sources into an outgoing data source.
    /// </summary>
    /// <typeparam name="TEntityIn">The data type of the incoming entity read from an underlying data source..</typeparam>
    /// <typeparam name="TEntityOut">The type of entity to process results to.</typeparam>
    public class ProcessorSingleResult<TEntityIn, TEntityOut> : IProcessor
        where TEntityOut : class, new()
    {
        readonly Logger _logger;
        private readonly Func<IEnumerable<TEntityIn>> _reader;
        private readonly ObjectsMapper<TEntityIn, TEntityOut> _mapper;
        private readonly IEnumerable<ISpecification<TEntityOut>> _rules;
        private IEnumerable<ISpecification<IEnumerable<TEntityOut>>> _collectionRules;

        /// <summary>
        ///     Gets or sets the working folder to persist temporary files to.
        /// </summary>
        public string WorkingFolder { get; set; }

        /// <summary>
        ///     Gets the correlation id.
        /// </summary>
        public Guid CorrelationId { get; set; }
        /// <summary>
        ///     Gets the batch id.
        /// </summary>
        public int BatchId { get; set; }

        /// <summary>
        ///     Raised after processing.
        /// </summary>

        public event EventHandler<EventArgs> OnFinishedProcessing;

        /// <summary>
        ///     Called before processing an incoming dto to an outgoing dto.
        /// </summary>
        public Action<TEntityIn, TEntityOut> BeginProcessingItem { get; set; }

        /// <summary>
        ///     Get the well known name of the processor type.
        /// </summary>

        public string ProcessName { get; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorSingleResult(
            Func<IEnumerable<TEntityIn>> reader,
            string processorName = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null,
            Logger logger = null,
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            _logger = logger ?? LogManager.GetCurrentClassLogger();
            _reader = reader;

            CorrelationId = SeqGuid.Create();
            BatchId = 1;
            ProcessName = processorName ?? $"{GetType().Name}-{typeof(TEntityOut).Name}";

            _mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();

            _rules = specProviderForEntity?.GetSpecifications().ToArray() ?? Enumerable.Empty<ISpecification<TEntityOut>>();
            _collectionRules = specProviderForEntityCollection?.GetSpecifications().ToArray() ?? Enumerable.Empty<ISpecification<IEnumerable<TEntityOut>>>();

        }

        protected IEnumerable<TEntityIn> Read() => _reader();

        /// <summary>
        ///     Creates a new process hanlder.
        /// </summary>
        /// <returns></returns>
        public ProcessorHandler<TEntityIn> CreateProcessHandler()
        {
            var processedValidItems = new List<TEntityOut>();

            var pHandler = new ProcessorHandler<TEntityIn>(ProcessName,CorrelationId, BatchId)
            {
                EntitiesReader = Read(),
                Logger = _logger,
                OnError = OnError,
                ProcessItem = (dtoIn) =>
                {
                    // create output entity
                    var dtoOut = new TEntityOut();

                    // apply default mapping
                    dtoOut = _mapper.Map(dtoIn, dtoOut);

                    // prepare entity
                    BeginProcessingItem?.Invoke(dtoIn, dtoOut); ;

                    ErrorEvent errorEvent = OnItemProcessing(dtoIn, dtoOut);

                    // validate against business rules
                    if (errorEvent == null)
                    {
                        var errors = _rules.ToErrors(dtoOut);
                        var e = errors as Error[] ?? errors.ToArray();
                        if (e.Any())
                        {
                            var first = e.First();
                            errorEvent = new ErrorEvent() { Message = first.Message, Type = first.Type };
                        }
                    }

                    if (errorEvent == null)
                        processedValidItems.Add(dtoOut);

                    return errorEvent;
                },
                Commit = () =>
                {
                    var errors = _collectionRules.ToErrors(processedValidItems);
                    var enumerable = errors as Error[] ?? errors.ToArray();
                    if (enumerable.Any())
                        throw new RuleException("Unable to commit items due to one or more rules. Please see the inner exception for more details.", enumerable);

                    Commit(processedValidItems);
                }
            };

            return pHandler;
        }

        /// <summary>
        ///     Gets the errors encountered processing from the incoming data source.
        /// </summary>
        public List<ProcessError<TEntityIn>> ProcessedErrors { get; private set; }

        /// <summary>
        ///     Processes a snapshot from an incoming source.
        /// </summary>
        public ProcessResult Process()
        {
            if (!Directory.Exists(WorkingFolder))
                Directory.CreateDirectory(WorkingFolder);

            ProcessorHandler<TEntityIn> handler = CreateProcessHandler();

            return Process(handler);
        }

        /// <summary>
        ///     Processes a snapshot of data using a process handle from an incoming source.
        /// </summary>
        /// <param name="processHandler">The handler to use.</param>
        /// <returns></returns>
        public ProcessResult Process(ProcessorHandler<TEntityIn> processHandler)
        {
            Guard.ArgumentNotNull(processHandler, nameof(processHandler));

            this.ProcessorHandler = processHandler;

            this.ProcessedItems = new List<TEntityOut>(); // clear

            ProcessResult result = processHandler.Process();

            this.ProcessedErrors = processHandler.Errors;

            // raise event
            OnFinishedProcessing?.Invoke(this, EventArgs.Empty);

            return result;
        }

        /// <summary>
        ///     Gets/sets the processor handler.
        /// </summary>
        public ProcessorHandler<TEntityIn> ProcessorHandler { get; private set; }

        /// <summary>
        ///     Processes a single item and returns and error event.
        /// </summary>
        public virtual ErrorEvent OnItemProcessing(TEntityIn dto, TEntityOut entityOut)
        {
            return null;
        }

        /// <summary>
        ///     Commit to the target valid items mapped/processed from the incoming data store.
        /// </summary>
        /// <param name="batch">The results to commit to the system.</param>
        public virtual void Commit(IEnumerable<TEntityOut> batch)
        {
            // save results to output file - unique
            var i = 1;
            var batchAsList = batch.ToList();

            var fileName = $@"{WorkingFolder}\{ProcessName}-OnFinishedProcessing-{CorrelationId}-{BatchId}.json";
            while (File.Exists(fileName))
                fileName = $@"{WorkingFolder}\{ProcessName}-{CorrelationId}-{BatchId}-{i++}.json";

            SaveSnapShot(fileName, batchAsList);

            this.ProcessedItems = batchAsList;
        }

        /// <summary>
        ///     Gets the valid items created after processing.
        /// </summary>
        public List<TEntityOut> ProcessedItems { get; private set; }

        /// <summary>
        ///     Raised whenever an error occured processign the results.
        /// </summary>
        /// <param name="entityIn">The entity that was being processing at the time the error was raised.</param>
        /// <param name="error">The error exception raised.</param>
        public virtual void OnError(TEntityIn entityIn, Exception error)
        {

        }

        /// <summary>
        ///     Saves a snapshot of the data to a given file.
        /// </summary>
        private void SaveSnapShot<T>(string filePath, List<T> data)
        {
            if (File.Exists(filePath))
            {
                // todo: log result
                File.Delete(filePath);
            }

            var log = new ProcessSnapshot<T>
            {
                CorrelationId = CorrelationId,
                Data = data
            };

            var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());

            File.WriteAllText(filePath, body);
        }
    }
}