using System;

namespace FutureState.Data.Keys
{
    /// <summary>
    ///     Responsible for generating a new id for a given key type.
    /// </summary>
    public interface IKeyGetter
    {
        
    }

    /// <summary>
    ///     Responsible for generating a new id for a given key type.
    /// </summary>
    public interface IKeyGetter<TKey> : IKeyGetter
    {
        TKey GetNew();
    }

    /// <summary>
    ///     Generic key generator for a given key type.
    /// </summary>
    public class KeyGetter<TKey> : IKeyGetter<TKey>
    {
        readonly Func<TKey> _getGet;

        public KeyGetter(Func<TKey> getGet)
        {
            Guard.ArgumentNotNull(getGet, nameof(getGet));

            _getGet = getGet;
        }

        public TKey GetNew()
        {
            return _getGet();
        }
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
        ///     Generates a new id for a given entity.
        /// </summary>
        void Provide(TEntity entity);
    }

    /// <summary>
    ///     Assigns a globally unique id to an entity that uses a single key.
    /// </summary>
    public interface IEntityIdProvider<in TEntity, TKey> : IEntityIdProvider // keep type arguments to support airity
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