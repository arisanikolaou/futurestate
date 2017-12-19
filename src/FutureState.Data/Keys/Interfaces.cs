using System;

namespace FutureState.Data.Keys
{
    /// <summary>
    ///     Responsible for generating a new id for a given key type.
    /// </summary>
    public interface IKeyGenerator
    {
    }

    /// <summary>
    ///     Responsible for generating a new id for a given key type.
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public interface IKeyGenerator<TEntity, out TKey> : IKeyGenerator // first type param required to distinguish instances in app domain
    {
        TKey GetNew();

        Type EntityType { get; }
    }

    /// <summary>
    ///     Generic key generator for a given key type.
    /// </summary>
    public class KeyGenerator<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        private readonly Func<TKey> _getGet;

        public Type EntityType { get; } = typeof(TEntity);

        public KeyGenerator(Func<TKey> getGet)
        {
            Guard.ArgumentNotNull(getGet, nameof(getGet));

            _getGet = getGet;
        }

        public TKey GetNew()
        {
            return _getGet();
        }
    }

    public class DefaultKeyGenerator<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        public Type EntityType { get; } = typeof(TEntity);

        public TKey GetNew() => default(TKey);
    }

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