#region

using System.Threading.Tasks;

#endregion

namespace FutureState.ComponentModel
{
    /// <summary>
    /// A pipe to send domain events through (such as a given service bus).
    /// </summary>
    public interface IMessagePipe
    {
        /// <summary>
        /// Asynchronously sends a message of type T to any subscribed consumers
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Task to notify when message was fully consumed</returns>
        Task SendAsync<T>(T message)
            where T : IDomainEvent;
    }
}