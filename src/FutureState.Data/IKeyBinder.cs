namespace FutureState.Data
{
    /// <summary>
    ///     Binds entity keys to an from a given entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity to bind to.</typeparam>
    /// <typeparam name="TKey">The key type to bind.</typeparam>
    public interface IKeyBinder<in TEntity, TKey>
    {
        /// <summary>
        ///     Gets the active entity key.
        /// </summary>
        TKey Get(TEntity entity);

        /// <summary>
        ///     Sets a key value to a given entity.
        /// </summary>
        void Set(TEntity entity, TKey key);
    }
}