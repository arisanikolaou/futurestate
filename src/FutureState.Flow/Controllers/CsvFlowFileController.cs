using System;

namespace FutureState.Flow.Controllers
{
    /// <summary>
    ///     Uses csv files to create batchs of data to process.
    /// </summary>
    /// <typeparam name="TIn">The input entity type.</typeparam>
    /// <typeparam name="TOut">The entity type to produce.</typeparam>
    public class CsvFlowFileController<TIn, TOut> : FlowFileController<TIn, TOut>
        where TOut : class, new()
    {
        public CsvFlowFileController(
            Func<IFlowFileController, Processor<TIn, TOut>> getProcessor = null,
            ProcessorConfiguration<TIn, TOut> config = null)
            : base(new CsvProcessorReader<TIn>(), getProcessor, config)
        {
        }
    }
}