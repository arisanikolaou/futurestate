#region

using System.Collections.Generic;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Something that can insert,update and delete entities into a repository in batches.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to insert.</typeparam>
    public interface IBatchUpdater<in TEntity>
    {
        BatchUpdateResult LastUpdateResult { get; }

        /// <summary>
        ///     Updates a batch of entities in one transaction.
        /// </summary>
        void Update(IEnumerable<TEntity> entity);
    }
}