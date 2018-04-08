using FutureState.Flow.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;

namespace FutureState.Flow.Controllers
{
    /// <summary>
    ///     Controls how data is read from a set of data source files and processed into outgoing flow files.
    /// </summary>
    /// <typeparam name="TIn">The incoming data type to read from.</typeparam>
    /// <typeparam name="TOut">The outgoing entity type to produce.</typeparam>
    public class FlowFileController<TIn, TOut> : IFlowFileController
        where TOut : class, new()
    {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly Func<IFlowFileController, Processor<TIn, TOut>> _getFlowFileController;
        private readonly FlowFileLogRepo _flowFileLogRepo;
        private readonly IReader<TIn> _reader;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="config">Processor configuration settings.</param>
        /// <param name="reader">The reader to read incoming data from.</param>
        /// <param name="getController">Function to create a new procesor.</param>
        public FlowFileController(
            ProcessorConfiguration<TIn, TOut> config,
            IReader<TIn> reader,
            Func<IFlowFileController, Processor<TIn, TOut>> getController = null)
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            Guard.ArgumentNotNull(config, nameof(config));

            _getFlowFileController = getController;
            _flowFileLogRepo = new FlowFileLogRepo();

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_getFlowFileController == null)
                _getFlowFileController = controller => throw new NotImplementedException();

            Config = config;
            
            // assign name from type name by default
            ControllerName = $"{GetType().Name.Replace("`2", "")}-{typeof(TIn).Name}-{typeof(TOut).Name}";

            Flow = new FlowId(typeof(TOut).Name);

            _reader = reader;
        }

        /// <summary>
        ///     Gets the configuration to use to setup of a processor.
        /// </summary>
        public ProcessorConfiguration<TIn, TOut> Config { get; }

        /// <summary>
        ///     Gets the type of entity being processed.
        /// </summary>
        public FlowEntity SourceEntityType => FlowEntity.Create<TIn>();

        /// <summary>
        ///     Gets the type of entity that will processed.
        /// </summary>
        public FlowEntity TargetEntityType => FlowEntity.Create<TOut>();

        /// <summary>
        ///     Initializes the controller.
        /// </summary>
        public virtual void Initialize()
        {
            try
            {
                if (!Directory.Exists(Config.OutDirectory))
                    Directory.CreateDirectory(Config.OutDirectory);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to initialize controller configuration for controller '{ControllerName}'.",
                    ex);
            }
        }

        /// <summary>
        ///     Gets the controller name.
        /// </summary>
        public string ControllerName { get; set; }

        /// <summary>
        ///     Gets/sets the unique flow.
        /// </summary>
        public FlowId Flow { get; set; }


        IProcessorConfiguration IFlowFileController.Config => Config;

        /// <summary>
        ///     Process a file file within a given batch.
        /// </summary>
        /// <param name="flowFile">The address to read the primary source entities from.</param>
        /// <param name="flowBatch">The batch to process the file in.</param>
        /// <returns></returns>
        public virtual FlowSnapshot Process(FileInfo flowFile, FlowBatch flowBatch)
        {
            try
            {
                // read the incoming batch of data
                IEnumerable<TIn> incomingData = _reader.Read(flowFile.FullName);

                // create the processor to batch process it
                var processor = GetProcessor();

                // save results to output directory
                if (!Directory.Exists(Config.OutDirectory))
                    Directory.CreateDirectory(Config.OutDirectory);

                // process incoming data into a snapshot result
                FlowSnapShot<TOut> result = processor.Process(incomingData, flowBatch);
                
                // save results
                var outputRepository = new FlowSnapshotRepo<FlowSnapShot<TOut>>(Config.OutDirectory);

                var targetAddressId = outputRepository.Save(result);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to process flow file {flowFile.Name} due to an unexpected error. Batch process is {flowBatch}.",
                    ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual Processor<TIn, TOut> GetProcessor()
        {
            return _getFlowFileController(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}