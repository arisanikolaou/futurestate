using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using FutureState.Flow.Core;
using FutureState.Specifications;

namespace FutureState.Flow
{
    /// <summary>
    ///     A configuration setting that can be shared across multiple different
    ///     processor types.
    /// </summary>
    public class ProcessorConfiguration<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        public ProcessorConfiguration(
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null,
            IProcessResultRepository<ProcessResult> repository = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null)
        {
            Mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();

            Rules = specProviderForEntity?.GetSpecifications().ToArray() ??
                    Enumerable.Empty<ISpecification<TEntityOut>>();

            CollectionRules = specProviderForEntityCollection?.GetSpecifications().ToArray() ??
                              Enumerable.Empty<ISpecification<IEnumerable<TEntityOut>>>();

            Repository = repository ?? new ProcessResultRepository<ProcessResult>(Environment.CurrentDirectory);
        }

        /// <summary>
        ///     The repository to save flow/process results to.
        /// </summary>
        public IProcessResultRepository<ProcessResult> Repository { get; }
        /// <summary>
        ///     Gets the rules to process/validate outgoing entities.
        /// </summary>
        public IEnumerable<ISpecification<TEntityOut>> Rules { get; }
        /// <summary>
        ///     Gets the default mapper to use to map incoming entities to outgoing entities.
        /// </summary>
        public ObjectsMapper<TEntityIn, TEntityOut> Mapper { get; }
        /// <summary>
        ///     Gets the rules to use to validate a collection of entities.
        /// </summary>
        public IEnumerable<ISpecification<IEnumerable<TEntityOut>>> CollectionRules { get; }
    }
}