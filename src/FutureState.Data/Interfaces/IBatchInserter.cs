#region

using System.Collections.Generic;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Something that can insert entities into a repository in batches.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to insert.</typeparam>
    public interface IBatchInserter<in TEntity> : IInserter<IEnumerable<TEntity>>
    {
    }
}