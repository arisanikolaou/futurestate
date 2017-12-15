using System;

namespace FutureState.Data
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity, TKey> GetRepository<TEntity, TKey>(ISession session)
            where TEntity : class, IEntity<TKey>, new()
            where TKey : IEquatable<TKey>;
    }

    public interface IRepositoryFactory<TEntity, in TKey>
        where TEntity : class, IEntity<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        IRepository<TEntity, TKey> GetRepository(ISession session);
    }
}