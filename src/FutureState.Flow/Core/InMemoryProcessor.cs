using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.Core
{
    public class InMemoryProcessor<TEntityIn, TEntityOut> : ProcessorSingleResult<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public InMemoryProcessor(IEnumerable<TEntityIn> dataSource) : base(dataSource.ToList)
        {
            
        }

    }
}