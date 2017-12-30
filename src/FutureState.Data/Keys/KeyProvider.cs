namespace FutureState.Data
{
    /// <summary>
    ///     Provides a new identity to an entity given a key getter implementation.
    /// </summary>
    public class KeyProvider<TEntity, TKey> : IKeyProvider<TEntity, TKey>
    {
        private readonly IKeyGenerator<TEntity, TKey> _getKey;
        private readonly IKeyBinder<TEntity, TKey> _keyBinder;

        /// <summary>
        ///     Creates a new instance using a given key generator and key binder.
        /// </summary>
        /// <param name="keyGenerator">The generator for keys for a given entity.</param>
        /// <param name="keyBinder">The key binder to use.</param>
        public KeyProvider(IKeyGenerator<TEntity, TKey> keyGenerator, IKeyBinder<TEntity, TKey> keyBinder)
        {
            _getKey = keyGenerator;
            _keyBinder = keyBinder;
        }

        /// <summary>
        ///     Created a key provider the uses a given ket generator.
        /// </summary>
        /// <param name="keyGenerator">The key generator to use.</param>
        public KeyProvider(IKeyGenerator<TEntity, TKey> keyGenerator) : this(keyGenerator,
            new KeyBinderFromAttributes<TEntity, TKey>())
        {
        }

        /// <summary>
        ///     Creates a default (no pp) key generator.
        /// </summary>
        public KeyProvider() : this(new KeyGenerator<TEntity, TKey>(() => default(TKey)),
            new KeyBinderFromAttributes<TEntity, TKey>())
        {
        }

        /// <summary>
        ///     Gets and assigns a new key to a given entity.
        /// </summary>
        public void Provide(TEntity entity)
        {
            var key = _getKey.GetNew();
            _keyBinder.Set(entity, key);
        }

        /// <summary>
        ///     Gets the key for a given entity.
        /// </summary>
        public TKey GetKey(TEntity entity)
        {
            return _keyBinder.Get(entity);
        }
    }
}