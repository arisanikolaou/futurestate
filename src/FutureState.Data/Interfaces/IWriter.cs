using System.Collections.Generic;

namespace FutureState.Data
{
    /// <summary>
    ///     Insert,updates and deletes a set of entities to an underlying data store.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to write.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    public interface IWriter<in TEntity, in TKey> :
        IInserter<TEntity>,
        IUpdater<TEntity>,
        IDeleter<TEntity, TKey>,
        IInserter<IEnumerable<TEntity>>
    {
        /// <summary>
        ///     Deletes all entities present in the underlying data store.
        /// </summary>
        void DeleteAll();
    }
}