using FutureState.Flow.Core;

namespace FutureState.Flow.Flow
{
    public abstract class ProcessResultBatchProcessor<TIn, TOut> : FlowFileBatchProcessor<TIn, TOut>
        where TOut : class, new()
    {
        protected ProcessResultBatchProcessor() : base(GetReader())
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