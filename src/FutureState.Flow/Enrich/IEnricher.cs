using System;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    /// <summary>
    ///     Enriches the content of a given entity type.
    /// </summary>
    public interface IEnricher
    {
        /// <summary>
        ///     Gets the entity type used to enrich a given target.
        /// </summary>
        FlowEntity SourceEntityType { get; }

        /// <summary>
        ///     Gets the unique network address of the data source.
        /// </summary>
        string AddressId { get; }
    }

    /// <summary>
    ///     Enriches the content of a well known entity type.
    /// </summary>
    /// <typeparam name="TTarget">The data type to enrich.</typeparam>
    public interface IEnricher<TTarget> : IEnricher
    {
        /// <summary>
        ///     Gets the data structures used to enrich the target type.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEquatable<TTarget>> Get();

        /// <summary>
        ///     Enriches the 'whole' data type from the 'part' data type.
        /// </summary>
        /// <returns></returns>
        TTarget Enrich(IEquatable<TTarget> part, TTarget whole);

        /// <summary>
        ///     Gets all parts that can be used to enrich a given target instance.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEquatable<TTarget>> Find(TTarget target);
    }
}