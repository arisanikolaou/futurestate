using System;

namespace FutureState.Data.KeyBinders
{
    public sealed class AssignableEntityKeyBinder<TEntity, TKey> : IEntityKeyBinder<TEntity, TKey>
        where TEntity : IEntityMutableKey<TKey>
    {
        public TKey Get(TEntity entity)
        {
            try
            {
                return entity.Id; //don't check for null entity to avoid perf penalty
            }
            catch (NullReferenceException)
            {
                //assume null reference
                Guard.ArgumentNotNull(entity, nameof(entity));

                throw;
            }
        }

        void IEntityKeyBinder<TEntity, TKey>.Set(TEntity entity, TKey key)
        {
            try
            {
                entity.Id = key; //don't check for null entity
            }
            catch (NullReferenceException)
            {
                Guard.ArgumentNotNull(entity, nameof(entity)); //assume invalid input

                throw;
            }
        }
    }
}