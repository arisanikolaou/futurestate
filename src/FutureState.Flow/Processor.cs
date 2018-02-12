using FutureState.Specifications;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private readonly ProcessorConfiguration<TEntityIn, TEntityOut> _config;
        private Action<TEntityIn, TEntityOut> _beginProcessingItem;
        private Func<TEntityIn, IEnumerable<TEntityOut>> _createOutput;
        private Action<IEnumerable<TEntityOut>> _onCommitting;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="config">
        ///     The configuration to use to map and validate items.
        /// </param>
        /// <param name="engine">
        ///     If not supplied a default handler will be used.
        /// </param>
        public Processor(
            ProcessorConfiguration<TEntityIn, TEntityOut> config,
            ProcessorEngine<TEntityIn> engine = null)
        {
            Guard.ArgumentNotNull(config, nameof(config));

            _config = config;

            CreateOutput = dtoIn => new[] { new TEntityOut() };
            ProcessName = GetProcessName(this);
            Engine = engine ?? new ProcessorEngine<TEntityIn>();
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
        public Action<TEntityIn, TEntityOut> BeginProcessingItem
        {
            get => _beginProcessingItem;
            set
            {
                Guard.ArgumentNotNull(value, nameof(BeginProcessingItem));

                _beginProcessingItem = value;
            }
        }

        /// <summary>
        ///     Gets/sets the processor handler.
        /// </summary>
        public ProcessorEngine<TEntityIn> Engine { get; }

        /// <summary>
        ///     Called while comitting processed changes.
        /// </summary>
        public Action<IEnumerable<TEntityOut>> OnCommitting
        {
            get => _onCommitting;
            set
            {
                Guard.ArgumentNotNull(value, nameof(OnCommitting));

                _onCommitting = value;
            }
        }

        /// <summary>
        ///     Get the well known name of the processor type.
        /// </summary>
        public string ProcessName { get; }


        /// <summary>
        ///     Gets the default processor name.
        /// </summary>
        /// <param name="processor">The processor instance to calculate a name for.</param>
        /// <returns></returns>
        public static string GetProcessName(IProcessor processor)
        {
            return $"{processor.GetType().Name.Replace("`2", "")}-{typeof(TEntityIn).Name}-{typeof(TEntityOut).Name}";
        }

        /// <summary>
        ///     Processes an incoming data stream to an output.
        /// </summary>
        public FlowSnapShot<TEntityOut> Process(IEnumerable<TEntityIn> reader, FlowBatch process)
        {
            var result = new FlowSnapShot<TEntityOut>
            {
                ProcessName = ProcessName,
                TargetType = new FlowEntity(typeof(TEntityOut)),
                SourceType = new FlowEntity(typeof(TEntityIn))
            };

            return Process(reader, process, result);
        }

        /// <summary>
        ///     Raised after processing.
        /// </summary>
        public event EventHandler<EventArgs> OnFinishedProcessing;

        /// <summary>
        ///     Creates a new process engine instance using a given entity reader and core engine.
        /// </summary>
        /// <returns></returns>
        public ProcessorEngine<TEntityIn> BuildProcessEngine(
            IEnumerable<TEntityIn> reader,
            ProcessorEngine<TEntityIn> engine, 
            FlowSnapShot<TEntityOut> result)
        {
            var processedValidItems = new List<TEntityOut>();
            var notValidItems = new List<TEntityOut>();

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
                IEnumerable<TEntityOut> itemsToProcess;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (CreateOutput != null)
                    itemsToProcess = CreateOutput(dtoIn);
                else
                    itemsToProcess = new[] {new TEntityOut()};

                var errorEvents = new List<ErrorEvent>();
                foreach (TEntityOut dtoOutDefault in itemsToProcess)
                {
                    try
                    {
                        ProcessOutputItem(
                            dtoIn,
                            dtoOutDefault,
                            errorEvents,
                            processedValidItems,
                            notValidItems);
                    }
                    catch (Exception ex)
                    {
                        var errorEvent = new ErrorEvent { Message = ex.Message, Type = "Exception" };

                        errorEvents.Add(errorEvent);
                        notValidItems.Add(dtoOutDefault);

                        throw;
                    }
                }

                return errorEvents;
            };

            // commit operation for valid processed items
            // ReSharper disable once ImplicitlyCapturedClosure
            engine.Commit = () =>
            {
                // curry commit
                pCommit?.Invoke();

                // validate collection commit
                IEnumerable<Error> errors = _config.CollectionRules.ToErrors(processedValidItems);

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

        private void ProcessOutputItem(TEntityIn dtoIn, TEntityOut dtoOutDefault, List<ErrorEvent> errorEvents, List<TEntityOut> processedValidItems, List<TEntityOut> notValidItems)
        {
            // apply default mapping
            TEntityOut dtoOut = _config.Mapper.Map(dtoIn, dtoOutDefault);

            // prepare entity
            BeginProcessingItem?.Invoke(dtoIn, dtoOut);

            // validate
            var errorEvent = OnItemProcessing(dtoIn, dtoOut);

            // validate against business rules
            if (errorEvent == null)
            {
                var errors = _config.Rules.ToErrors(dtoOut);
                var errorsArray = errors as Error[] ?? errors.ToArray();
                if (errorsArray.Any())
                {
                    foreach (var error in errorsArray)
                    {
                        errorEvent = new ErrorEvent { Message = error.Message, Type = error.Type };
                        errorEvents.Add(errorEvent);
                    }

                    notValidItems.Add(dtoOut);
                }
                else
                {
                    processedValidItems.Add(dtoOut);
                }
            }
            else
            {
                errorEvents.Add(errorEvent);
                notValidItems.Add(dtoOut);
            }
        }

        /// <summary>
        ///     Processes a FlowBatch of data using a process handle from an incoming source.
        /// </summary>
        /// <param name="reader">The source for the incoming dtos.</param>
        /// <param name="process">The batch process to run.</param>
        /// <param name="resultState">The resultState state from processing.</param>
        /// <returns></returns>
        public FlowSnapShot<TEntityOut> Process(
            IEnumerable<TEntityIn> reader, 
            FlowBatch process,
            FlowSnapShot<TEntityOut> resultState)
        {
            if (Logger.IsTraceEnabled)
                Logger.Trace($"Starting to process  {ProcessName} batch {process.BatchId}.");

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
        public virtual void Commit(IEnumerable<TEntityOut> batch, FlowSnapShot<TEntityOut> result)
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