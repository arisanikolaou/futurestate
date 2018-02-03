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
        private ServiceAggregate<IInstrumentService> _aggregate;
        private IInstrumentService _service1;
        private IInstrumentService _service2;

        protected void GivenAServiceAggregate()
        {
            this._aggregate = new ServiceAggregate<IInstrumentService>(
                new IInstrumentService[] { new Service1(), new Service2(), new Service2() });
        }

        protected void WhenDemandingAServiceType()
        {
            this._service1 = _aggregate.Demand(m => m.Type == typeof(Bond));
        }

        protected void WhenDemandingAnotherServiceType()
        {
            this._service2 = _aggregate.Demand(m => m.Type == typeof(Equity));
        }

        protected void ThenDemandingANonExistingServiceTypeShouldThrowNotSupportedError()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                _aggregate.Demand(m => m.Type == typeof(NonExistant));
            });
        }

        protected void ThenServicesShouldBeResolved()
        {
            Assert.NotNull(_service1);
            Assert.Equal(typeof(Bond), _service1.Type);

            Assert.NotNull(_service2);
            Assert.Equal(typeof(Equity), _service2.Type);
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