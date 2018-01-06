using System;
using System.Linq.Expressions;

namespace FutureState.Data
{
    /// <summary>
    ///     Can delete a given entity from an underlying data store.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to delete.</typeparam>
    /// <typeparam name="TKey">The entity type key.</typeparam>
    public interface IDeleter<in TEntity, in TKey>
    {
        /// <summary>
        ///     Deletes a single entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(TEntity entity);

        /// <summary>
        ///     Deletes an entity by its key.
        /// </summary>
        /// <param name="key">The key value.</param>
        void DeleteById(TKey key);
    }

    /// <summary>
    ///     Deletes a guid keyed entity from an underlying data store.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to delete.</typeparam>
    public interface IDeleter<in TEntity> : IDeleter<TEntity, Guid>
    {
    }

    /// <summary>
    ///     Deletes a set of records by a given predicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity's key type.</typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IBulkDeleter<TEntity, in TKey> //do not delete 'redundant' key
    {
        void Delete(Expression<Func<TEntity, bool>> predicate);
    }

    /// <summary>
    ///     Deletes a set of guid keyed entities by a given predicate.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IBulkDeleter<T> : IBulkDeleter<T, Guid> //do not delete 'redundant' key
    {
    }
}