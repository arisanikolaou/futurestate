#region

using System;
using System.Collections.Generic;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Reads all or a single entity by its key.
    /// </summary>
    public interface IReader<out TEntity, in TKey> : IGetter<TEntity, TKey>
    {
        /// <summary>
        ///     Gets all entities in the underlying repository.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        ///     Gets whether there are any items in the underlying collection.
        /// </summary>
        bool Any();

        /// <summary>
        ///     Gets the number of entities in the underlying collection.
        /// </summary>
        long Count();

        /// <summary>
        ///     Gets all entities matching the given entity ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetByIds(IEnumerable<TKey> ids);
    }

    /// <summary>
    ///     Reads all or a single entity by its key.
    /// </summary>
    public interface IReader<out TEntity> : IReader<TEntity, Guid>, IGetter<TEntity>
    {
    }
}