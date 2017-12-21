using FutureState.Data.Core;

namespace FutureState.Data.Core
{
    /// <summary>
    ///     AssignedGenerator is a pass-through implementation that assumes the IDs are managed by the caller. This is
    ///     basically no op id generator.
    /// </summary>
    /// <typeparam name="TEntity">The entity to bind the id value to.</typeparam>
    public class NoOpEntityIdProvider<TEntity, TKey> : IEntityIdProvider<TEntity, TKey>
    {
        private readonly IEntityKeyBinder<TEntity, TKey> _binder;

        public NoOpEntityIdProvider()
            : this(new AttributeKeyBinder<TEntity, TKey>())
        {
        }

        public NoOpEntityIdProvider(IEntityKeyBinder<TEntity, TKey> binder)
        {
            Guard.ArgumentNotNull(binder, nameof(binder));

            _binder = binder;
        }

        /// <summary>
        ///     Gets the key of the entity.
        /// </summary>
        TKey IEntityIdProvider<TEntity, TKey>.GetKey(TEntity entity)
        {
            return _binder.Get(entity);
        }

        void IEntityIdProvider<TEntity, TKey>.Provide(TEntity entity)
        {
            //do nothing
        }
    }
}