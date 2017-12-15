using System;

namespace FutureState.Data.Providers
{
    public abstract class DomainEvent<TItem> : IDomainEvent
    {
        public TItem Item { get; }

        protected DomainEvent(TItem entity)
        {
            Guard.ArgumentNotNull(entity, "entity");

            Item = entity;
        }
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
        public TKey Key { get; }

        public Type Type => typeof(TItem);

        public ItemDeleted(TKey key)
        {
            Guard.ArgumentNotNull(key, "key");

            Key = key;
        }
    }
}
