using System;
using System.Collections.Generic;
using System.Linq;
using FutureState.Specifications;
using NLog;

namespace FutureState.Flow
{
    /// <summary>
    ///     Processes a single type of entity from an incoming data 
    ///     sources into an outgoing data source.
    /// </summary>
    /// <typeparam name="TEntityIn">The data type of the incoming entity read from an underlying data source.</typeparam>
    /// <typeparam name="TEntityOut">The type of entity to process results to.</typeparam>
    public class Processor<TEntityIn, TEntityOut> : IProcessor
        where TEntityOut : class, new()
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ProcessorConfiguration<TEntityIn, TEntityOut> _configuration;
        private Func<TEntityIn, IEnumerable<TEntityOut>> _createOutput;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Processor(
            ProcessorConfiguration<TEntityIn, TEntityOut> configuration,
            string processorName = null)
        {
            _configuration = configuration;
            processorName = processorName ?? GetProcessName(this);

            CreateOutput = dtoIn => new[] {new TEntityOut()};

            Engine = new ProcessorEngine<TEntityIn>(
                processorName,
                _configuration.Repository);
        }

        /// <summary>
        ///     Action to create an outgoing dto(s) from an incoming dto.
        /// </summary>
        public Func<TEntityIn, IEnumerable<TEntityOut>> CreateOutput
        {
            get => _createOutput;
            set
            {
                Guard.ArgumentNotNull(value, nameof(CreateOutput));

                _createOutput = value;
            }
        }

        /// <summary>
        ///     Called before processing an incoming dto to an outgoing dto.
        /// </summary>
        public Action<TEntityIn, TEntityOut> BeginProcessingItem { get; set; }

        /// <summary>
        ///     Gets/sets the processor handler.
        /// </summary>
        public ProcessorEngine<TEntityIn> Engine { get; }

        /// <summary>
        ///     Called while comitting processed changes.
        /// </summary>
        public Action<IEnumerable<TEntityOut>> OnCommitting { get; set; }

        /// <summary>
        ///     Get the well known name of the processor type.
        /// </summary>
        public string ProcessName => Engine.ProcessName;

        public static string GetProcessName(IProcessor processor)
        {
            return $"{processor.GetType().Name}-{typeof(TEntityOut).Name}";
        }

        /// <summary>
        ///     Processes an incoming data stream to an output.
        /// </summary>
        public ProcessResult<TEntityIn, TEntityOut> Process(IEnumerable<TEntityIn> reader, BatchProcess process)
        {
            var result = new ProcessResult<TEntityIn, TEntityOut>();

            return Process(reader, process, result);
        }

        /// <summary>
        ///     Raised after processing.
        /// </summary>
        public event EventHandler<EventArgs> OnFinishedProcessing;


        /// <summary>
        ///     Creates a new process hanlder.
        /// </summary>
        /// <returns></returns>
        public ProcessorEngine<TEntityIn> BuildProcessEngine(IEnumerable<TEntityIn> reader,
            ProcessorEngine<TEntityIn> engine, ProcessResult<TEntityIn, TEntityOut> result)
        {
            var processedValidItems = new List<TEntityOut>();

            engine.EntitiesReader = reader;
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
                IEnumerable<TEntityOut> itemsToProcess = new[] {new TEntityOut()};
                if (CreateOutput != null)
                    itemsToProcess = CreateOutput(dtoIn);

                var errorEvents = new List<ErrorEvent>();
                foreach (var item in itemsToProcess)
                {
                    // apply default mapping
                    var dtoOut = _configuration.Mapper.Map(dtoIn, item);

                    // prepare entity
                    BeginProcessingItem?.Invoke(dtoIn, dtoOut);

                    var errorEvent = OnItemProcessing(dtoIn, dtoOut);

                    // validate against business rules
                    if (errorEvent == null)
                    {
                        var errors = _configuration.Rules.ToErrors(dtoOut);
                        var e = errors as Error[] ?? errors.ToArray();
                        if (e.Any())
                            foreach (var error in e)
                            {
                                errorEvent = new ErrorEvent {Message = error.Message, Type = error.Type};
                                errorEvents.Add(errorEvent);
                            }
                        else
                            processedValidItems.Add(dtoOut);
                    }
                }

                return errorEvents;
            };
            // commit operation for valid processed items
            engine.Commit = () =>
            {
                // curry commit
                pCommit?.Invoke();

                // validate collection commit
                var errors = _configuration.CollectionRules.ToErrors(processedValidItems);

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
        ///     Processes a BatchProcess of data using a process handle from an incoming source.
        /// </summary>
        /// <param name="reader">The source for the incoming dtos.</param>
        /// <param name="process">The batch process to run.</param>
        /// <param name="resultState">The resultState state from processing.</param>
        /// <returns></returns>
        public ProcessResult<TEntityIn, TEntityOut> Process(IEnumerable<TEntityIn> reader, BatchProcess process,
            ProcessResult<TEntityIn, TEntityOut> resultState)
        {
            if (Logger.IsTraceEnabled)
                Logger.Trace($"Starting to process  {ProcessName} Batch {process.BatchId}.");

            try
            {
                // setup the processing engine
                BuildProcessEngine(reader, Engine, resultState);

                // process all items
                Engine.Process(process, resultState);

                // raise event finished
                OnFinishedProcessing?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(
                    $"Failed to process {ProcessName} batch {process.BatchId} due to an unexpected error.", ex);
            }
            finally
            {
                if (Logger.IsTraceEnabled)
                    Logger.Trace($"Finished processing {ProcessName} batch {process.BatchId}.");
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
    }
}