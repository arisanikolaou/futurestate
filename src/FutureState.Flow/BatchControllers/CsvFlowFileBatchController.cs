using System;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Uses csv files to create batchs of data to process.
    /// </summary>
    /// <typeparam name="TIn">The input entity type.</typeparam>
    /// <typeparam name="TOut">The entity type to produce.</typeparam>
    public class CsvFlowFileFlowFileBatchController<TIn, TOut> : FlowFileFlowFileBatchController<TIn, TOut>
        where TOut : class, new()
    {
        public CsvFlowFileFlowFileBatchController(
            Func<IFlowFileBatchController, Processor<TIn, TOut>> getProcessor = null,
            ProcessorConfiguration<TIn, TOut> config = null)
            : base(new CsvProcessorReader<TIn>(), getProcessor, config)
        {
        }
    }
}