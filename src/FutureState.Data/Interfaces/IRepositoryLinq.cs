using System;

namespace FutureState.Data
{
    public interface IRepositoryLinq<TEntity, in TKey> :
        IRepository<TEntity, TKey>,
        ILinqReader<TEntity, TKey>,
        IBulkDeleter<TEntity, TKey>
    {
    }

    public interface IRepositoryLinq<TEntity> :
        IRepository<TEntity, Guid>,
        ILinqReader<TEntity, Guid>,
        IBulkDeleter<TEntity, Guid>
    {
    }
}