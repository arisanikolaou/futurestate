using FutureState.Data.KeyBinders;

namespace FutureState.Data.Keys
{
    /// <summary>
    ///     Provides a new identity to an entity given a key getter implementation.
    /// </summary>
    public class EntityIdProvider<TEntity, TKey> : IEntityIdProvider<TEntity, TKey>
    {
        private readonly IKeyGenerator<TEntity, TKey> _getKey;
        private readonly IEntityKeyBinder<TEntity, TKey> _keyBinder;

        public EntityIdProvider(IKeyGenerator<TEntity, TKey> keyGenerator, IEntityKeyBinder<TEntity, TKey> keyBinder)
        {
            _getKey = keyGenerator;
            _keyBinder = keyBinder;
        }

        public EntityIdProvider(IKeyGenerator<TEntity, TKey> keyGenerator) : this(keyGenerator, new AttributeKeyBinder<TEntity, TKey>())
        {

        }

        public void Provide(TEntity entity)
        {
            TKey key = _getKey.GetNew();
            _keyBinder.Set(entity, key);
        }
        
        public TKey GetKey(TEntity entity)
        {
            return _keyBinder.Get(entity);
        }
    }
}