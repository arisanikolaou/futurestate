#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace FutureState.ComponentModel
{
    /// <summary>
    ///     A generic message pipe to send message of type TDomainEvent within a given application domain.
    /// </summary>
    /// <remarks>
    ///     The object is thread friendly.
    /// </remarks>
    public class MessagePipe : IMessagePipe, IDisposable
    {
        private readonly HashSet<IMessageConsumer> _subscriptions;

        private readonly object _syncLock = new object();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public MessagePipe()
        {
            _subscriptions = new HashSet<IMessageConsumer>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public void Dispose()
        {
            //lock subscriptions
            lock (_syncLock)
            {
                _subscriptions.Clear();
            }
        }

        /// <summary>
        ///     Asynchronously sends a message of type TDomainEvent to any subscribed consumers.
        /// </summary>
        public virtual async Task SendAsync<TDomainEvent>(TDomainEvent message)
            where TDomainEvent : IDomainEvent
        {
            Guard.ArgumentNotNull(message, nameof(message));

            var subscribers = GetSubscribers<TDomainEvent>();

            var tasks = subscribers.Select(c => c.ConsumeAsync(message));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        ///     Registers a new consumer of the given message.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public MessagePipe Subscribe<TDomainEvent>(IMessageConsumer<TDomainEvent> consumer)
            where TDomainEvent : IDomainEvent
        {
            Guard.ArgumentNotNull(consumer, nameof(consumer));

            lock (_syncLock)
            {
                _subscriptions.Add(consumer);
            }

            return this;
        }

        /// <summary>
        ///     Registers a new consumer of the given message.
        /// </summary>
        /// <param name="consumer">The consumer or owner of the event.</param>
        /// <param name="action">The action to execute.</param>
        public MessagePipe Subscribe<TDomainEvent>(Action<TDomainEvent> action, object consumer)
            where TDomainEvent : IDomainEvent
        {
            Guard.ArgumentNotNull(consumer, nameof(consumer));
            Guard.ArgumentNotNull(action, nameof(action));

            lock (_syncLock)
            {
                _subscriptions.Add(new MessageConsumer<TDomainEvent>(action, consumer));
            }

            return this;
        }

        /// <summary>
        ///     Unregisters the consumer from the internal list.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        /// <returns>True if the consumer was registered and removed.</returns>
        public bool UnSubscribe<TDomainEvent>(IMessageConsumer<TDomainEvent> consumer)
            where TDomainEvent : IDomainEvent
        {
            Guard.ArgumentNotNull(consumer, nameof(consumer));

            // ReSharper disable once LoopCanBeConvertedToQuery
            lock (_syncLock)
            {
                if (_subscriptions.Contains(consumer))
                {
                    _subscriptions.Remove(consumer);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Unregisters the consumer from the internal list.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        /// <returns>True if the consumer was registered and removed.</returns>
        public bool UnSubscribe<TDomainEvent>(object consumer)
            where TDomainEvent : IDomainEvent
        {
            Guard.ArgumentNotNull(consumer, nameof(consumer));

            // ReSharper disable once LoopCanBeConvertedToQuery
            lock (_syncLock)
            {
                //has hash code override
                var messageConsumer = new MessageConsumer<TDomainEvent>(consumer);
                if (!_subscriptions.Contains(messageConsumer))
                    return false;

                _subscriptions.Remove(messageConsumer);

                return true;
            }
        }

        /// <summary>
        ///     Unsubscribes all messages owned by the given consumer.
        /// </summary>
        public bool UnSubscribeAll(object consumer)
        {
            Guard.ArgumentNotNull(consumer, nameof(consumer));

            lock (_syncLock)
            {
                var ownedSubscriptions =
                    _subscriptions.OfType<IOwnedMessageConsumer>().Where(m => m.Owner.Equals(consumer)).ToList();

                //remove all subscriptions
                ownedSubscriptions.Each(messageConsumer => _subscriptions.Remove(messageConsumer));
            }

            return false;
        }

        /// <summary>
        ///     Gets a list of all subscribers for messages of type TDomainEvent.
        /// </summary>
        protected List<IMessageConsumer<TDomainEvent>> GetSubscribers<TDomainEvent>()
            where TDomainEvent : IDomainEvent
        {
            List<IMessageConsumer<TDomainEvent>> consumers;

            lock (_syncLock)
            {
                consumers = _subscriptions
                    .OfType<IMessageConsumer<TDomainEvent>>()
                    .ToList(); //copy list
            }

            return consumers;
        }

        //don't make public
        private interface IOwnedMessageConsumer : IMessageConsumer
        {
            object Owner { get; }
        }

        //don't make public
        internal class MessageConsumer<TDomainEvent> : IMessageConsumer<TDomainEvent>, IOwnedMessageConsumer
            where TDomainEvent : IDomainEvent
        {
            private readonly int _hashCode;

            internal MessageConsumer(Action<TDomainEvent> action, object owner)
                : this(owner)
            {
                Action = action;
            }

            internal MessageConsumer(object owner)
            {
                Action = domainEvent => throw new NotSupportedException();
                Owner = owner;

                unchecked
                {
                    var fullName = typeof(TDomainEvent).FullName;
                    if (fullName != null)
                        _hashCode = fullName.GetHashCode() ^ Owner.GetHashCode();
                }
            }

            public Action<TDomainEvent> Action { get; }

            /// <summary>
            ///     Consumes a domain event.
            /// </summary>
            /// <param name="domainEvent">The domain event to consume.</param>
            /// <returns></returns>
            public Task ConsumeAsync(TDomainEvent domainEvent)
            {
                return Task.Run(() => Action?.Invoke(domainEvent));
            }

            public object Owner { get; }

            /// <summary>
            ///     Determines whether the specified object is equal to the current object.
            /// </summary>
            /// <returns>
            ///     true if the specified object  is equal to the current object; otherwise, false.
            /// </returns>
            /// <param name="obj">The object to compare with the current object. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(obj, null))
                    return false;

                if (obj is MessageConsumer<TDomainEvent> consumer)
                    return consumer.GetHashCode() == _hashCode;

                return ReferenceEquals(this, obj);
            }

            /// <summary>
            ///     Serves as the default hash function.
            /// </summary>
            /// <returns>
            ///     A hash code for the current object.
            /// </returns>
            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}