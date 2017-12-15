using System;

namespace FutureState.Data.Keys
{
    /// <summary>
    /// Id generate for entity types with a long primary key.
    /// </summary>
    public sealed class EntityIdProviderForLong<TEntity> : EntityIdProvider<TEntity, long>
    {
        // ReSharper disable once StaticMemberInGenericType
        static long _current;

        // ReSharper disable once StaticMemberInGenericType
        static readonly Func<long> _getNext = () => _current++;

        public EntityIdProviderForLong(long initial)
            : base(new KeyGetter<long>(() => _getNext() + initial))
        {
            
        }

        public EntityIdProviderForLong(long initial, IEntityKeyBinder<TEntity, long> binder) :
            base(new KeyGetter<long>(() => _getNext() + initial), binder)
        {
        }
    }
}