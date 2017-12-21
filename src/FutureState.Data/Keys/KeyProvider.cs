namespace FutureState.Data
{
    /// <summary>
    ///     Provides a new identity to an entity given a key getter implementation.
    /// </summary>
    public class KeyProvider<TEntity, TKey> : IKeyProvider<TEntity, TKey>
    {
        private readonly IKeyGenerator<TEntity, TKey> _getKey;
        private readonly IKeyBinder<TEntity, TKey> _keyBinder;

        public KeyProvider(IKeyGenerator<TEntity, TKey> keyGenerator, IKeyBinder<TEntity, TKey> keyBinder)
        {
            _getKey = keyGenerator;
            _keyBinder = keyBinder;
        }

        public KeyProvider(IKeyGenerator<TEntity, TKey> keyGenerator) : this(keyGenerator,
            new KeyBinderFromAttributes<TEntity, TKey>())
        {
        }

        public void Provide(TEntity entity)
        {
            var key = _getKey.GetNew();
            _keyBinder.Set(entity, key);
        }

        public TKey GetKey(TEntity entity)
        {
            return _keyBinder.Get(entity);
        }
    }
}