#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState
{
    /// <summary>
    ///     A basic domain event dispatcher.
    /// </summary>
    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly List<Action<IDomainEvent>> _dispatchers;

        public DomainEventDispatcher(IEnumerable<Action<IDomainEvent>> dispatchers)
        {
            _dispatchers = dispatchers.ToList();
        }

        public void Dispatch(IDomainEvent domainEvent)
        {
            Guard.ArgumentNotNull(domainEvent, nameof(domainEvent));

            _dispatchers.Each(
                m => { m?.Invoke(domainEvent); });
        }
    }
}