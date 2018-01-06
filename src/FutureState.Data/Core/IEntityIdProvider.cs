namespace FutureState.Data.Core
{

    /// <summary>
    ///     Assigns a globally unique id to an entity.
    /// </summary>
    public interface IEntityIdProvider
    {
    }

    /// <summary>
    ///     Assigns a globally unique id to an entity.
    /// </summary>
    public interface IEntityIdProvider<in TEntity> : IEntityIdProvider
    {
        /// <summary>
        ///     Generates and assigns a new key to a given entity.
        /// </summary>
        void Provide(TEntity entity);
    }

    /// <summary>
    ///     Assigns a globally unique id to an entity that uses a single key.
    /// </summary>
    public interface IEntityIdProvider<in TEntity, out TKey> : IEntityIdProvider // keep type arguments to support airity
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