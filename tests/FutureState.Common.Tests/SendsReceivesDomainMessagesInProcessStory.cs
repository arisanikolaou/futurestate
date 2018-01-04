using FutureState.ComponentModel;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Common.Tests
{
    [Story(AsA = "As a developer using the api.",
        IWant = "I want to be able to send/receive domain events via an in process message pipe.",
        SoThat = "So that i can manage loosely coupled yet reactive services within an app.")]
    public class SendsReceivesDomainMessagesInProcessStory
    {
        private MessagePipe subject;
        DomainEvent evt;
        private int hitCount;

        public void GivenAMessagePipe()
        {
            this.subject = new MessagePipe();
        }

        public void WhenSendingAMessageWithoutASubscriber()
        {
            subject.SendAsync(new DomainEvent() { Name = "Name" }).Wait();
        }

        public void AndWhenSubscribingToAnEvent()
        {
            subject.Subscribe<DomainEvent>((received) =>
            {
                hitCount++;
                evt = received;
            }, this);
        }

        public void AndWhenAMessageSubscribedToIsSent()
        {
            subject.SendAsync(new DomainEvent() { Name = "Name" }).Wait();
        }

        public void AndWhenAnotherDerivedMessageTypeIsSent()
        {
            subject.SendAsync(new DomainEvent2() { Name = "Name 2" }).Wait();
        }

        public void ThenSubscribedEventShouldBeReceivedAfterItIsSent()
        {
            Assert.NotNull(evt);
            Assert.Equal("Name", evt.Name);
            Assert.Equal(1, hitCount);
        }

        public void AndThenShouldBeAbleToUnSubcribeToEvent()
        {
            subject.UnSubscribe<DomainEvent>(this);

            subject.SendAsync(new DomainEvent() { Name = "Name 2" }).Wait();

            // should be the same
            Assert.Equal("Name", evt.Name);
            Assert.Equal(1, hitCount);
        }

        [BddfyFact]
        public void SendsReceivesDomainMessagesInProcess()
        {
            this.BDDfy();
        }

        public class DomainEvent : IDomainEvent
        {
            public string Name { get; set; }
        }

        public class DomainEvent2 : IDomainEvent
        {
            public string Name { get; set; }
        }
    }
}
