
namespace FutureState.Flow.BatchControllers
{
    public abstract class CsvFlowFileFlowFileBatchController<TIn, TOut> : FlowFileFlowFileBatchController<TIn, TOut>
        where TOut : class, new()
    {
        protected CsvFlowFileFlowFileBatchController() : base(new CsvProcessorReader<TIn>())
        {

        }
    }
}
