using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Uses supplied functions to get/set the key of the entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity's key type.</typeparam>
    public class KeyBinder<TEntity, TKey> : IKeyBinder<TEntity, TKey>
    {
        private readonly Func<TEntity, TKey> _getter;

        private readonly Action<TEntity, TKey> _setter;

        /// <summary>
        ///     Creates a new instance using a given getter and setter.
        /// </summary>
        public KeyBinder(Func<TEntity, TKey> getter, Action<TEntity, TKey> setter)
        {
            Guard.ArgumentNotNull(getter, nameof(getter));
            Guard.ArgumentNotNull(setter, nameof(setter));

            _getter = getter;
            _setter = setter;
        }

        public TKey Get(TEntity entity)
        {
            return _getter(entity);
        }

        public void Set(TEntity entity, TKey key)
        {
            _setter(entity, key);
        }
    }
}