namespace FutureState.Data
{
    /// <summary>
    ///     Assigns a globally unique id to an entity.
    /// </summary>
    public interface IKeyProvider
    {
    }

    /// <summary>
    ///     Assigns a globally unique id to an entity.
    /// </summary>
    public interface IKeyProvider<in TEntity> : IKeyProvider
    {
        /// <summary>
        ///     Generates and assigns a new key to a given entity.
        /// </summary>
        void Provide(TEntity entity);
    }

    /// <summary>
    ///     Assigns a globally unique id to an entity that uses a single key.
    /// </summary>
    public interface IKeyProvider<in TEntity, out TKey> : IKeyProvider // keep type arguments to support airity
    {
        /// <summary>
        ///     Gets the key of the entity.
        /// </summary>
        TKey GetKey(TEntity entity);

        /// <summary>
        ///     Generates a new id for a given entity.
        /// </summary>
        void Provide(TEntity entity);
    }
}