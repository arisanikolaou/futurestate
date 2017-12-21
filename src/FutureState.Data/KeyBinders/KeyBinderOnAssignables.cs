namespace FutureState.Data
{
    public sealed class KeyBinderOnAssignables<TEntity, TKey> : IKeyBinder<TEntity, TKey>
        where TEntity : IEntityMutableKey<TKey>
    {
        public TKey Get(TEntity entity)
        {
            return entity.Id; //don't check for null entity to avoid perf penalty
        }

        public void Set(TEntity entity, TKey key)
        {
            entity.Id = key; //don't check for null entity
        }
    }
}