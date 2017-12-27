using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Creates repositories that will operate against a given data store session.
    /// </summary>
    public interface IRepositoryFactory
    {
        IRepository<TEntity, TKey> GetRepository<TEntity, TKey>(ISession session)
            where TEntity : class, IEntity<TKey>, new()
            where TKey : IEquatable<TKey>;
    }
}