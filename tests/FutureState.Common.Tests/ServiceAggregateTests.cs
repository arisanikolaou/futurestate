using FutureState.ComponentModel;
using System;
using System.Collections.Generic;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Common.Tests
{
    [Story]
    public class ServiceAggregateTests
    {
        private ServiceAggregate<IInstrumentService> aggregate;
        private IInstrumentService service1;
        private IInstrumentService service2;

        protected void GivenAServiceAggregate()
        {
            this.aggregate = new ServiceAggregate<IInstrumentService>(
                new IInstrumentService[] { new Service1(), new Service2() , new Service2() });
        }

        protected void WhenDemandingAServiceType()
        {
            this.service1 = aggregate.Demand(m => m.Type == typeof(Bond));
        }

        protected void WhenDemandingAnotherServiceType()
        {
            this.service2 = aggregate.Demand(m => m.Type == typeof(Equity));
        }

        protected void ThenDemandingANonExistingServiceTypeShouldThrowNotSupportedError()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                aggregate.Demand(m => m.Type == typeof(NonExistant));
            });
        }

        protected void ThenServicesShouldBeResolved()
        {
            Assert.NotNull(service1);
            Assert.Equal(typeof(Bond), service1.Type );

            Assert.NotNull(service2);
            Assert.Equal(typeof(Equity), service2.Type);
        }

        [BddfyFact]
        public void AggregatesAndResolvesRegisteredServices()
        {
            this.BDDfy();
        }

        public interface IInstrumentService
        {
            IEnumerable<object> Select();

            Type Type { get; }
        }

        public class Service1 : IInstrumentService
        {
            public Type Type { get; set; } = typeof(Equity);

            public IEnumerable<object> Select()
            {
                yield break;
            }
        }

        public class Service2 : IInstrumentService
        {
            public Type Type { get; set; } = typeof(Bond);

            public IEnumerable<object> Select()
            {
                yield break;
            }
        }

        public class Instrument
        {

        }

        public class Bond
        {

        }

        public class Equity
        {

        }

        public class NonExistant
        {

        }
    }
}