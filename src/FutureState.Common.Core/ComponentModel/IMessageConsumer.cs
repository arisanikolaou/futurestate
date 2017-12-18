#region

using System.Threading.Tasks;

#endregion

namespace FutureState.ComponentModel
{
    /// <summary>
    ///     Consumes messages dispatched over a given message pipe of a given type.
    /// </summary>
    public interface IMessageConsumer<in TDomainEvent> : IMessageConsumer
        where TDomainEvent : IDomainEvent
    {
        /// <summary>
        ///     Consumes a message asynchronously.
        /// </summary>
        /// <param name="domainEvent">Message to consume.</param>
        /// <returns>Task to let caller know when the message was consumed.</returns>
        Task ConsumeAsync(TDomainEvent domainEvent);
    }

    /// <summary>
    ///     Consumes messages over a given message pipe.
    /// </summary>
    public interface IMessageConsumer
    {
    }
}