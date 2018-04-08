using FutureState.Flow.Data;
using System;

namespace FutureState.Flow.Controllers
{
    /// <summary>
    ///     Reads the entities successfully processed by another processor.
    /// </summary>
    /// <typeparam name="TIn">The entity type to read in.</typeparam>
    /// <typeparam name="TOut">The entity type to process out.</typeparam>
    public class FlowSnapshotFileController<TIn, TOut> : FlowFileController<TIn, TOut>
        where TOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="config">The configuration to use to map, configure incoming to outgoing entities.</param>
        /// <param name="getProcessor">How to get processors.</param>
        public FlowSnapshotFileController(
            ProcessorConfiguration<TIn, TOut> config,
            Func<IFlowFileController, Processor<TIn, TOut>> getProcessor = null)
            : base(config, GetReader(), getProcessor)
        {
        }

        /// <summary>
        ///     Gets the reader function.
        /// </summary>
        /// <returns></returns>
        private static IReader<TIn> GetReader()
        {
            return new GenericResultReader<TIn>(flowFileSource =>
            {
                var repoository = new FlowSnapshotRepo<FlowSnapShot<TIn>>(flowFileSource);

                var processResult = repoository.Get(flowFileSource);

                return processResult.Valid;
            });
        }
    }
}