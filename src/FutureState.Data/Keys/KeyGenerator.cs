using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Generic key generator for a given key type.
    /// </summary>
    public class KeyGenerator<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        private readonly Func<TKey> _getGet;

        /// <summary>
        ///     Creates a new instance that uses a given function to generate new keys for a given entity type.
        /// </summary>
        public KeyGenerator(Func<TKey> getGet)
        {
            Guard.ArgumentNotNull(getGet, nameof(getGet));

            _getGet = getGet;
        }

        /// <summary>
        ///     Creates a new instance that generates default keys.
        /// </summary>
        public KeyGenerator() : this(() => default(TKey))
        {
            
        }

        /// <summary>
        ///     Gets the entity type keys should be generated for.
        /// </summary>
        public Type EntityType { get; } = typeof(TEntity);

        /// <summary>
        ///     Gets a new key for a given entity.
        /// </summary>
        public TKey GetNew()
        {
            return _getGet();
        }
    }
}