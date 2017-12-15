using System;

namespace FutureState
{
    /// <summary>
    /// Dispatches a set of domain events in a given application domain.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a given domain event such as order processed or simulation completed.
        /// </summary>
        /// <exception cref="ArgumentNullException">Raised if the given domain event is null.</exception>
        void Dispatch(IDomainEvent domainEvent);
    }
}