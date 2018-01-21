using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using FutureState.Specifications;
using NLog;

namespace FutureState.Flow.Core
{

    /// <summary>
    ///     Processes a single type in income entity data sources into an outgoing data source.
    /// </summary>
    /// <typeparam name="TEntityIn">The data type of the incoming entity read from an underlying data source..</typeparam>
    /// <typeparam name="TEntityOut">The type of entity to process results to.</typeparam>
    public class ProcessorService<TEntityIn, TEntityOut> : IProcessor
        where TEntityOut : class, new()
    {
        private readonly IEnumerable<ISpecification<IEnumerable<TEntityOut>>> _collectionRules;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ObjectsMapper<TEntityIn, TEntityOut> _mapper;
        private readonly Func<IEnumerable<TEntityIn>> _reader;
        private readonly IEnumerable<ISpecification<TEntityOut>> _rules;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorService(
            Func<IEnumerable<TEntityIn>> reader,
            Guid? correlationId = null,
            long batchId = 1,
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null,
            IProcessResultRepository<ProcessResult> repository = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null,
            string processorName = null)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            _reader = reader;

            correlationId = correlationId ?? SeqGuid.Create();
            processorName = processorName ?? $"{GetType().Name}-{typeof(TEntityOut).Name}";

            _mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();

            _rules = specProviderForEntity?.GetSpecifications().ToArray() ??
                     Enumerable.Empty<ISpecification<TEntityOut>>();

            _collectionRules = specProviderForEntityCollection?.GetSpecifications().ToArray() ??
                               Enumerable.Empty<ISpecification<IEnumerable<TEntityOut>>>();

            Engine = new ProcessorEngine<TEntityIn>(correlationId, batchId, repository, processorName)
            {
                Logger = _logger
            };
        }


        /// <summary>
        ///     Called before processing an incoming dto to an outgoing dto.
        /// </summary>
        public Action<TEntityIn, TEntityOut> BeginProcessingItem { get; set; }

        /// <summary>
        ///     Gets/sets the processor handler.
        /// </summary>
        public ProcessorEngine<TEntityIn> Engine { get;  }

        /// <summary>
        ///     Get the well known name of the processor type.
        /// </summary>
        public string ProcessName => this.Engine.ProcessName;
        
        /// <summary>
        ///     Called while comitting processed changes.
        /// </summary>
        public Action<IEnumerable<TEntityOut>> OnCommitting { get; set; }

        /// <summary>
        ///     Processes an incoming data stream to an output.
        /// </summary>
        public ProcessResult<TEntityIn, TEntityOut> Process()
        {
            var result = new ProcessResult<TEntityIn,TEntityOut>();

            return Process(result);
        }

        /// <summary>
        ///     Raised after processing.
        /// </summary>
        public event EventHandler<EventArgs> OnFinishedProcessing;

        /// <summary>
        ///     Reads data from the incoming source to the processor.
        /// </summary>
        protected virtual IEnumerable<TEntityIn> Read()
        {
            return _reader();
        }

        /// <summary>
        ///     Creates a new process hanlder.
        /// </summary>
        /// <returns></returns>
        public ProcessorEngine<TEntityIn> BuildProcessEngine(ProcessorEngine<TEntityIn> engine, ProcessResult<TEntityIn, TEntityOut> result)
        {
            var processedValidItems = new List<TEntityOut>();

            engine.EntitiesReader = Read();
            engine.OnError = OnError;

            // curry methods
            var pItem = engine.ProcessItem;
            var pCommit = engine.Commit;

            // function to validate and process one item
            engine.ProcessItem = dtoIn =>
            {
                // chain methods
                pItem?.Invoke(dtoIn);

                // create output entity
                var dtoOut = new TEntityOut();

                // apply default mapping
                dtoOut = _mapper.Map(dtoIn, dtoOut);

                // prepare entity
                BeginProcessingItem?.Invoke(dtoIn, dtoOut);
                ;

                var errorEvent = OnItemProcessing(dtoIn, dtoOut);

                // validate against business rules
                if (errorEvent == null)
                {
                    var errors = _rules.ToErrors(dtoOut);
                    var e = errors as Error[] ?? errors.ToArray();
                    if (e.Any())
                    {
                        var first = e.First();
                        errorEvent = new ErrorEvent { Message = first.Message, Type = first.Type };
                    }
                }

                if (errorEvent == null)
                    processedValidItems.Add(dtoOut);

                return errorEvent;
            };
            // commit operation for valid processed items
            engine.Commit = () =>
            {
                // curry commit
                pCommit?.Invoke();

                // validate collection commit
                var errors = _collectionRules.ToErrors(processedValidItems);

                var enumerable = errors as Error[] ?? errors.ToArray();
                if (enumerable.Any())
                    throw new RuleException(
                        "Unable to commit items due to one or more rules. Please see the inner exception for more details.",
                        enumerable);

                OnCommitting?.Invoke(processedValidItems);

                Commit(processedValidItems, result);
            };

            return engine;
        }

        /// <summary>
        ///     Processes a snapshot of data using a process handle from an incoming source.
        /// </summary>
        /// <param name="resultState">The resultState state from processing.</param>
        /// <returns></returns>
        public ProcessResult<TEntityIn,TEntityOut> Process(ProcessResult<TEntityIn, TEntityOut> resultState)
        {
            if(_logger.IsTraceEnabled)
                _logger.Trace($"Starting to process  {ProcessName} Batch {Engine.BatchId}.");

            try
            {
                // setup the processing engine
                BuildProcessEngine(Engine, resultState);

                // process all items
                Engine.Process(resultState);

                // raise event finished
                OnFinishedProcessing?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to process {ProcessName} batch {Engine.BatchId} due to an unexpected error.", ex);
            }
            finally
            {
                if (_logger.IsTraceEnabled)
                    _logger.Trace($"Finished processing {ProcessName} batch {Engine.BatchId}.");
            }

            return resultState;
        }

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
        /// <param name="result">The state to store the operation results to.</param>
        public virtual void Commit(IEnumerable<TEntityOut> batch, ProcessResult<TEntityIn, TEntityOut> result)
        {
            // save results to output file - unique
            var batchAsList = batch.ToList();

            result.Output = batchAsList;
        }

        /// <summary>
        ///     Raised whenever an error occured processign the results.
        /// </summary>
        /// <param name="entityIn">The entity that was being processing at the time the error was raised.</param>
        /// <param name="error">The error exception raised.</param>
        public virtual void OnError(TEntityIn entityIn, Exception error)
        {
        }

        ProcessResult IProcessor.Process() => this.Process();
    }
}