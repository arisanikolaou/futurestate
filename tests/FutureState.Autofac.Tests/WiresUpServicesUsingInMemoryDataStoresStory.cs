using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FutureState.Autofac.Modules;
using FutureState.Data;
using FutureState.Data.Providers;
using FutureState.Reflection;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Autofac.Tests
{
    [Story]
    public class WiresUpServicesUsingInMemoryDataStoresStory : IDisposable
    {
        private IContainer _container;
        private IEnumerable<Contact> _items;
        private IEnumerable<Address> _itemsFromGuidKeyedEntity;


        protected void GivenAnInMemoryDrivenApp()
        {
            var cb = new ContainerBuilder();

            cb.RegisterModule(new GenericDataServiceModule());
            cb.RegisterModule(new InMemoryDataAccessModule());
            cb.RegisterAll(new AppTypeScanner(Environment.CurrentDirectory, "FutureState.Autofac"));

            _container = cb.Build();
        }

        protected void WhenAddingAnEntityToTheApp()
        {
            var entity = new Contact
            {
                Id = 1,
                Name = "Name"
            };

            var provider = _container.Resolve<ProviderLinq<Contact, int>>();
            provider.Add(entity);
        }

        protected void AndWhenQueryingPopulatedEntities()
        {
            var provider = _container.Resolve<ProviderLinq<Contact, int>>();
            _items = provider.GetAll();
        }

        protected void AndWhenResolvingEntitiesFromUnPopulatedEntities()
        {
            var provider = _container.Resolve<ProviderLinq<Address, Guid>>();
            _itemsFromGuidKeyedEntity = provider.GetAll();
        }

        protected void ThenShouldBeAbleToQueryItemsFromProviders()
        {
            Assert.Single(_items, m => m.Name == "Name");
            Assert.Empty(_itemsFromGuidKeyedEntity);
        }

        [BddfyFact]
        public void WiresUpServicesUsingInMemoryDataStores()
        {
            this.BDDfy();
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}