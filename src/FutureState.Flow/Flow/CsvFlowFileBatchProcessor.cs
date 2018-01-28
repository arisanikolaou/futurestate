using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureState.Flow.Flow
{
    public abstract class CsvFlowFileBatchProcessor<TIn, TOut> : FlowFileBatchProcessor<TIn, TOut>
        where TOut : class, new()
    {
        protected CsvFlowFileBatchProcessor() : base(new CsvProcessorReader<TIn>())
        {

        }
    }
}
