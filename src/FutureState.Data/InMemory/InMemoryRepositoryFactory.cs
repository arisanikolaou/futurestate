using System;
using System.Collections.Concurrent;

namespace FutureState.Data
{
    /// <summary>
    ///     A factory that creates in-memory databases for entities of any given type that are
    ///     all responsible for generating their own global unique identififers.
    /// </summary>
    public class InMemoryRepositoryFactory : IRepositoryFactory
    {
        private readonly ConcurrentDictionary<Type, object> _repositories;

        public InMemoryRepositoryFactory()
        {
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>(ISession session)
            where TEntity : class, IEntity<TKey>, new()
            where TKey : IEquatable<TKey>
        {
            return GetRepository<TEntity, TKey>();
        }

        public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
            where TEntity : class, IEntity<TKey>, new()
            where TKey : IEquatable<TKey>
        {
            var entityType = typeof(TEntity);

            return _repositories.GetOrAdd(entityType,
                type =>
                    new InMemoryRepository<TEntity, TKey>(
                        new KeyProviderNoOp<TEntity, TKey>(),
                        new KeyBinderFromAttributes<TEntity, TKey>(),
                        new TEntity[0])) as IRepository<TEntity, TKey>;
        }

        #region Implementation of IUnconstrainedRepositoryFactory

        public IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>(ISession session) where TEntity : class, new()
            where TKey : IEquatable<TKey>
        {
            return _repositories.GetOrAdd(typeof(TEntity),
                type =>
                    new InMemoryRepository<TEntity, TKey>(
                        new KeyProviderNoOp<TEntity, TKey>(),
                        new KeyBinderFromAttributes<TEntity, TKey>(),
                        new TEntity[0])) as IRepository<TEntity, TKey>;
        }

        public IRepository<TEntity> CreateRepository<TEntity>(ISession session) where TEntity : class, new()
        {
            return _repositories.GetOrAdd(typeof(TEntity),
                type =>
                    new InMemoryRepository<TEntity>(
                        new KeyProviderNoOp<TEntity, Guid>(),
                        new KeyBinderFromAttributes<TEntity, Guid>(),
                        new TEntity[0])) as IRepository<TEntity>;
        }

        #endregion Implementation of IUnconstrainedRepositoryFactory
    }
}