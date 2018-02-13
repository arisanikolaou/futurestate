using EmitMapper;
using FutureState.Specifications;
using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     A configuration setting that can be shared across multiple different
    ///     processor types.
    /// </summary>
    public class ProcessorConfiguration<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        private readonly IProvideSpecifications<TEntityOut> _specProviderForEntity;
        private readonly IProvideSpecifications<IEnumerable<TEntityOut>> _specProviderForEntityCollection;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="specProviderForEntity"></param>
        /// <param name="specProviderForEntityCollection"></param>
        /// <param name="mapper">The mapper to use to map incoming entities to outgoing entities.</param>
        public ProcessorConfiguration(
            IProvideSpecifications<TEntityOut> specProviderForEntity,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null)
        {
            Guard.ArgumentNotNull(specProviderForEntity, nameof(specProviderForEntity));
            Guard.ArgumentNotNull(specProviderForEntityCollection, nameof(specProviderForEntityCollection));

            _specProviderForEntity = specProviderForEntity;
            _specProviderForEntityCollection = specProviderForEntityCollection;

            Mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();
        }

        /// <summary>
        ///     Gets the rules to process/validate outgoing entities.
        /// </summary>
        public IEnumerable<ISpecification<TEntityOut>> Rules => _specProviderForEntity.GetSpecifications();

        /// <summary>
        ///     Gets the default mapper to use to map incoming entities to outgoing entities.
        /// </summary>
        public ObjectsMapper<TEntityIn, TEntityOut> Mapper { get; }

        /// <summary>
        ///     Gets the rules to use to validate a collection of materialized entities.
        /// </summary>
        public IEnumerable<ISpecification<IEnumerable<TEntityOut>>> CollectionRules =>
            _specProviderForEntityCollection.GetSpecifications();
    }
}