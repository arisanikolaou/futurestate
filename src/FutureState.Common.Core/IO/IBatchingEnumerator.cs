#region

using System.Collections.Generic;

#endregion

namespace FutureState.IO
{
    /// <summary>
    /// Abstracts logic to stream and batch through a given data source.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IBatchingEnumerator<out TEntity>
    {
        /// <summary>
        /// Gets the batch size to be used.
        /// </summary>
        int BatchSize { get; }

        /// <summary>
        /// Gets the current batch to enumerate through.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetCurrentItems();

        /// <summary>
        /// Moves to the next batch.
        /// </summary>
        bool MoveNext();
    }
}