using System;
using FutureState.Flow.Core;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Reads the entities successfully processed by another processor.
    /// </summary>
    /// <typeparam name="TIn">The entity type to read in.</typeparam>
    /// <typeparam name="TOut">The entity type to process out.</typeparam>
    public class ProcessResultFlowFileBatchController<TIn, TOut> : FlowFileFlowFileBatchController<TIn, TOut>
        where TOut : class, new()
    {
        public ProcessResultFlowFileBatchController(
            Func<IFlowFileBatchController, Processor<TIn, TOut>> getProcessor = null,
            ProcessorConfiguration<TIn, TOut> config = null) 
            : base(GetReader(), getProcessor, config)
        {

        }

        static IReader<TIn> GetReader()
        {
            return new GenericResultReader<TIn>((flowFileSource) =>
            {
                var repoository = new ProcessResultRepository<ProcessResult<TOut, TIn>>(flowFileSource);

                var processResult = repoository.Get(flowFileSource);

                return processResult.Output;
            });
        }
    }
}