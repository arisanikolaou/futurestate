using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using FutureState.Specifications;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data streams from an in-memory datasource.
    /// </summary>
    public class InMemoryProcessor<TEntityIn, TEntityOut> : ProcessorService<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public InMemoryProcessor(
            IEnumerable<TEntityIn> dataSource,
            Guid? correlationId = null,
            long batchId = 1,
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null,
            IProcessResultRepository<ProcessResult> repository = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null,
            string processorName = null) : base(
            dataSource.ToList,
            correlationId,
            batchId,
            specProviderForEntity,
            specProviderForEntityCollection,
            repository,
            mapper,
            processorName)
        {

        }
    }
}