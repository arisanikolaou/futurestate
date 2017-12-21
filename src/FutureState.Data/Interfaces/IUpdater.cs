using System;
using System.Collections.Generic;

namespace FutureState.Data
{
    /// <summary>
    ///     Updates a single or multiple set of entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity to update.</typeparam>
    public interface IUpdater<in TEntity>
    {
        /// <summary>
        ///     Updates the given entity.
        /// </summary>
        /// <param name="item">Required. The entity to update.</param>
        /// <returns>
        ///     True if one entity was affected.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Raised if entity is null.
        /// </exception>
        void Update(TEntity item);

        /// <summary>
        ///     Updates a batch of entities in one transaction.
        /// </summary>
        void Update(IEnumerable<TEntity> entities);
    }
}