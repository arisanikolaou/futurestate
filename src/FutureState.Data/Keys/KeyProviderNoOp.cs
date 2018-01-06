namespace FutureState.Data
{
    /// <summary>
    ///     AssignedGenerator is a pass-through implementation that assumes the IDs are managed by the caller. This is
    ///     basically no op id generator.
    /// </summary>
    /// <typeparam name="TEntity">The entity to bind the id value to.</typeparam>
    /// <typeparam name="TKey">The key type of the entity.</typeparam>
    public class KeyProviderNoOp<TEntity, TKey> : IKeyProvider<TEntity, TKey>
    {
        private readonly IKeyBinder<TEntity, TKey> _binder;

        public KeyProviderNoOp()
            : this(new KeyBinderFromAttributes<TEntity, TKey>())
        {
        }

        public KeyProviderNoOp(IKeyBinder<TEntity, TKey> binder)
        {
            Guard.ArgumentNotNull(binder, nameof(binder));

            _binder = binder;
        }

        /// <summary>
        ///     Gets the key of the entity.
        /// </summary>
        TKey IKeyProvider<TEntity, TKey>.GetKey(TEntity entity)
        {
            return _binder.Get(entity);
        }

        void IKeyProvider<TEntity, TKey>.Provide(TEntity entity)
        {
        }
    }
}