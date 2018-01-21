using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data streams from an in-memory datasource.
    /// </summary>
    public class InMemoryProcessor<TEntityIn, TEntityOut> : Processor<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public InMemoryProcessor(
            IEnumerable<TEntityIn> dataSource,
            ProcessorConfiguration<TEntityIn, TEntityOut> configuration,
            string processorName = null) : base(
            dataSource.ToList,
            configuration,
            processorName)
        {

        }
    }
}