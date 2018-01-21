using System;
using System.Collections.Generic;
using EmitMapper;
using FutureState.Specifications;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data received from a csv file.
    /// </summary>
    /// <typeparam name="TEntityIn">The type of entity to read in from the underlying data source.</typeparam>
    /// <typeparam name="TEntityOut">The type of entity that will be produced after processing.</typeparam>
    public class CsvProcessor<TEntityIn, TEntityOut> : ProcessorService<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CsvProcessor(
            string dataSource,
            Guid? correlationId = null,
            long batchId = 1,
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null,
            IProcessResultRepository<ProcessResult> repository = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null,
            string processorName = null) : base(
                () => new CsvProcessorReader<TEntityIn>(dataSource).Read(),
                correlationId,
                batchId,
                specProviderForEntity,
                specProviderForEntityCollection,
                repository,
                mapper,
                processorName)
        {
            DataSource = dataSource;
        }

        /// <summary>
        ///     Gets or sets the file path to read data from.
        /// </summary>
        public string DataSource { get; }
    }
}