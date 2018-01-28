using FutureState.Flow.Core;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Reads the entities successfully processed by another processor.
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public abstract class ProcessResultFlowFileBatchController<TIn, TOut> : FlowFileFlowFileBatchController<TIn, TOut>
        where TOut : class, new()
    {
        protected ProcessResultFlowFileBatchController() : base(GetReader())
        {


        }

        static IReader<TIn> GetReader()
        {
            return new GenericResultReader<TIn>((dataSource) =>
            {
                var repoository = new ProcessResultRepository<ProcessResult<TOut, TIn>>(dataSource);

                var processResult = repoository.Get(dataSource);

                return processResult.Output;
            });
        }
    }
}