using System;

namespace FutureState.Data.KeyBinders
{
    /// <summary>
    /// ExpressionKeyBinder uses supplied functions to access primary key of the entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey">The entity's key type.</typeparam>
    public class ExpressionKeyBinder<TEntity, TKey> : IEntityKeyBinder<TEntity, TKey>
    {
        readonly Func<TEntity, TKey> _getter;

        readonly Action<TEntity, TKey> _setter;

        /// <summary>
        ///     Creates a new instance using a given getter and setter.
        /// </summary>
        public ExpressionKeyBinder(Func<TEntity, TKey> getter, Action<TEntity, TKey> setter)
        {
            Guard.ArgumentNotNull(getter, nameof(getter));
            Guard.ArgumentNotNull(setter, nameof(setter));

            _getter = getter;
            _setter = setter;
        }

        public TKey Get(TEntity entity)
        {
            try
            {
                return _getter(entity);
            }
            catch (NullReferenceException)
            {
                Guard.ArgumentNotNull(entity, nameof(entity));

                throw;
            }
        }

        public void Set(TEntity entity, TKey key)
        {
            try
            {
                _setter(entity, key);
            }
            catch (NullReferenceException)
            {
                Guard.ArgumentNotNull(entity, nameof(entity));

                throw;
            }
        }
    }
}