using FutureState.Data.Keys;
using System;
using System.Collections.Concurrent;
using FutureState.Data.KeyBinders;

namespace FutureState.Data
{
    /// <summary>
    /// A factory that creates in-memory databases for entities of any given type that are
    /// all responsible for generating their own global unique identififers.
    /// </summary>
    public class InMemoryRepositoryFactory : IRepositoryFactory
    {
        readonly ConcurrentDictionary<Type, object> _repositories;

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
                        new NoOpEntityIdProvider<TEntity, TKey>(),
                        new AttributeKeyBinder<TEntity, TKey>(),
                        new TEntity[0])) as IRepository<TEntity, TKey>;
        }

        #region Implementation of IUnconstrainedRepositoryFactory

        public IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>(ISession session) where TEntity : class, new()
            where TKey : IEquatable<TKey>
        {
            return _repositories.GetOrAdd(typeof(TEntity),
                type =>
                    new InMemoryRepository<TEntity, TKey>(
                        new NoOpEntityIdProvider<TEntity, TKey>(),
                        new AttributeKeyBinder<TEntity, TKey>(),
                        new TEntity[0])) as IRepository<TEntity, TKey>;
        }

        public IRepository<TEntity> CreateRepository<TEntity>(ISession session) where TEntity : class, new()
        {
            return _repositories.GetOrAdd(typeof(TEntity),
                type =>
                    new InMemoryRepository<TEntity>(
                        new NoOpEntityIdProvider<TEntity, Guid>(),
                        new AttributeKeyBinder<TEntity, Guid>(),
                        new TEntity[0])) as IRepository<TEntity>;
        }

        #endregion Implementation of IUnconstrainedRepositoryFactory
    }
}