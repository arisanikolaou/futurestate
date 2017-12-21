using System;

namespace FutureState.Data.Providers
{
    /// <summary>
    ///     Helps process an entity through a pipe/filter.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle.</typeparam>
    /// <typeparam name="TKey">The entity's system key type.</typeparam>
    public class EntityHandler<TEntity, TKey>
    {
        public EntityHandler(
            IActiveHandler<TEntity> activateHandler,
            IAddHandler<TEntity> addHandler,
            IRemoveHandler<TKey> removeHandler)
        {
            ActivateHandler = activateHandler ?? throw new ArgumentNullException(nameof(activateHandler));
            AddHandler = addHandler ?? throw new ArgumentNullException(nameof(addHandler));
            RemoveHandler = removeHandler ?? throw new ArgumentNullException(nameof(removeHandler));
        }

        public IAddHandler<TEntity> AddHandler { get; }
        public IRemoveHandler<TKey> RemoveHandler { get; }
        public IActiveHandler<TEntity> ActivateHandler { get; }
    }

    public interface IRemoveHandler<in TKey>
    {
        void Handle(TKey entity);
    }

    public interface IAddHandler<in TEntity>
    {
        void Handle(TEntity entity);
    }

    public interface IActiveHandler<in TEntity>
    {
        void Handle(TEntity entity);
    }
}