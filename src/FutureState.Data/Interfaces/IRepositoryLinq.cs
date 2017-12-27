using System;

namespace FutureState.Data
{
    /// <summary>
    ///     A repository that supports Linq style expressions.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    public interface IRepositoryLinq<TEntity, in TKey> :
        IRepository<TEntity, TKey>,
        ILinqReader<TEntity, TKey>,
        IBulkDeleter<TEntity, TKey>
    {
    }

    /// <summary>
    ///     A repository that supports Linq style expressions for guid keyed entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IRepositoryLinq<TEntity> :
        IRepository<TEntity, Guid>,
        ILinqReader<TEntity, Guid>,
        IBulkDeleter<TEntity, Guid>
    {
    }
}