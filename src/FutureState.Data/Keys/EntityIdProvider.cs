using FutureState.Data.KeyBinders;

namespace FutureState.Data.Keys
{
    /// <summary>
    ///     Provides a new identity to an entity given a key getter implementation.
    /// </summary>
    public class EntityIdProvider<TEntity, TKey> : IEntityIdProvider<TEntity, TKey>
    {
        readonly IEntityKeyBinder<TEntity, TKey> _keyBinder;
        readonly IKeyGetter<TKey> _getKey;

        public EntityIdProvider(IKeyGetter<TKey> keyGetter, IEntityKeyBinder<TEntity, TKey> keyBinder)
        {
            _getKey = keyGetter;
            _keyBinder = keyBinder;
        }

        public EntityIdProvider(IKeyGetter<TKey> keyGetter) : this(keyGetter, new AttributeKeyBinder<TEntity, TKey>())
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
    };
}
