using System;

namespace FutureState
{
    public abstract class DomainEvent<TItem> : IDomainEvent
    {
        protected DomainEvent(TItem entity)
        {
            Guard.ArgumentNotNull(entity, "entity");

            Item = entity;
        }

        public TItem Item { get; }
    }

    public sealed class ItemAdded<TItem> : DomainEvent<TItem>
    {
        public ItemAdded(TItem entity) : base(entity)
        {
        }
    }

    public sealed class ItemUpdated<TItem> : DomainEvent<TItem>
    {
        public ItemUpdated(TItem entity) : base(entity)
        {
        }
    }

    // ReSharper disable once UnusedTypeParameter
    public sealed class ItemDeleted<TItem, TKey> : IDomainEvent
    {
        public ItemDeleted(TKey key)
        {
            Guard.ArgumentNotNull(key, "key");

            Key = key;
        }

        public TKey Key { get; }

        public Type Type => typeof(TItem);
    }
}