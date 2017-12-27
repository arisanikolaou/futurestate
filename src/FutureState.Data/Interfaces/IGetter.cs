#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Gets an entity by its key from an underlying data store.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The type of the entity's key.</typeparam>
    public interface IGetter<out TEntity, in TKey>
    {
        /// <summary>
        ///     Gets a single entity by its unique identifier.
        /// </summary>
        /// <returns>Null if no matching entity exists.</returns>
        TEntity Get(TKey key);
    }

    /// <summary>
    ///     Gets an entity by its key from an underlying data store.
    /// </summary>
    public interface IGetter<out TEntity> : IGetter<TEntity, Guid>
    {
    }
}