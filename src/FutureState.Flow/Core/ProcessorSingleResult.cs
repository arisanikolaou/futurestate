using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmitMapper;
using Newtonsoft.Json;
using NLog;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes a single type in income entity data sources into an outgoing data source.
    /// </summary>
    /// <typeparam name="TEntityIn">The data type of the incoming entity.</typeparam>
    /// <typeparam name="TEntityOut">The type of the processed entity.</typeparam>
    public class ProcessorSingleResult<TEntityIn, TEntityOut> : IProcessor
        where TEntityOut : class , new()
    {
        readonly Logger _logger;
        private readonly Func<IEnumerable<TEntityIn>> _reader;
        private readonly ObjectsMapper<TEntityIn, TEntityOut> _mapper;

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

        public event EventHandler<EventArgs> Processed;
        /// <summary>
        ///     Get the well known name of the processor type.
        /// </summary>

        public string ProcessorType { get; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorSingleResult(Func<IEnumerable<TEntityIn>> reader, string processorType = null, ObjectsMapper<TEntityIn, TEntityOut>  mapper = null, Logger logger = null)
        {
            Guard.ArgumentNotNull(reader , nameof(reader));

            _logger = logger ?? LogManager.GetCurrentClassLogger();
            _reader = reader;

            ProcessorType = processorType ?? $"{GetType().Name}-{typeof(TEntityOut).Name}";

            _mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();
        }

        protected IEnumerable<TEntityIn> Read() => _reader();

        /// <summary>
        ///     Creates a new process hanlder.
        /// </summary>
        /// <returns></returns>
        public ProcessorHandler<TEntityIn> CreateProcessHandler()
        {
            var validProcessItems = new List<TEntityOut>();

            var pHandler = new ProcessorHandler<TEntityIn>
            {
                EntitiesReader = Read(),
                Logger = _logger,
                OnError = OnError,
                ProcessItem = (dto) =>
                {
                    var newEntity = new TEntityOut();

                    // apply default mapping
                    newEntity = _mapper.Map(dto, newEntity);

                    var result = ProcessItem(dto, newEntity);

                    if(result ==null)
                        validProcessItems.Add(newEntity);

                    return result;
                },
                Commit = () => Commit(validProcessItems)
            };

            return pHandler;
        }

        /// <summary>
        ///     Processes a snapshot from an incoming source.
        /// </summary>
        /// <returns></returns>
        public ProcessOperationResult Process()
        {
            var pHandler = CreateProcessHandler();

            return Process(pHandler);
        }


        /// <summary>
        ///     Processes a snapshot of data using a process handle from an incoming source.
        /// </summary>
        /// <param name="pHandler">The handler to use.</param>
        /// <returns></returns>
        public ProcessOperationResult Process(ProcessorHandler<TEntityIn> pHandler)
        {
            Guard.ArgumentNotNull(pHandler, nameof(pHandler));

            this.ProcessorHandler = pHandler;

            var result = pHandler.Process();

            Processed?.Invoke(this, EventArgs.Empty);

            return result;
        }

        /// <summary>
        ///     Gets/sets the processor handler.
        /// </summary>
        public ProcessorHandler<TEntityIn> ProcessorHandler { get; private set; }

        /// <summary>
        ///     Processes a single item and returns and error event.
        /// </summary>
        public virtual ErrorEvent ProcessItem(TEntityIn dto, TEntityOut entityOut)
        {
            return null;
        }

        /// <summary>
        ///     Commit to the target valid items mapped/processed from the incoming data store.
        /// </summary>
        /// <param name="batch">The results to commit to the system.</param>
        public virtual void Commit(IEnumerable<TEntityOut> batch)
        {
            // save results to output file
            var i = 0;
            var fileName = $"{GetType().Name}-Processed-{CorrelationId}-{BatchId}.json";
            while (File.Exists(fileName))
                fileName = $"{GetType().Name}-{CorrelationId}-{BatchId}-{i}.json";

            SaveSnapShot(fileName, batch.ToList());
        }

        /// <summary>
        ///     Raised whenever an error occured processign the results.
        /// </summary>
        /// <param name="entityIn"></param>
        /// <param name="error"></param>
        public virtual void OnError(TEntityIn entityIn, Exception error)
        {

        }

        private void SaveSnapShot<T>(string fileName, List<T> data)
        {
            var log = new ProcessSnapshot<T>
            {
                CorrelationId = CorrelationId,
                Data = data
            };

            var body = JsonConvert.SerializeObject(log, new JsonSerializerSettings());
            File.WriteAllText(fileName, body);
        }
    }
}