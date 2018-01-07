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
    public class WiresUpServicesStory : IDisposable
    {
        int contactId = 0;
        private string _conString;
        private IContainer _container;
        private string _dbName;
        private IEnumerable<Contact> _items;
        private IEnumerable<Address> _itemsFromGuidKeyedEntity;

        public void Dispose()
        {
            var dbSetup = new LocalDbSetup(Environment.CurrentDirectory, _dbName);
            dbSetup.TryDetachDatabase();
        }

        private string GetLocalDbConString(string dbName)
        {
            string baseDirectory = Environment.CurrentDirectory;

            string serverName = LocalDbSetup.LocalDbServerName;
            var conString =
                $@"data source={serverName};AttachDBFilename={baseDirectory}\{dbName}.mdf;initial catalog={
                        dbName
                    };integrated security=True;MultipleActiveResultSets=True;App=UnitTests";

            return conString;
        }

        protected void GivenABasicSqlDatabase()
        {
            var entity = new Contact
            {
                Id = 1,
                Name = "Name"
            };

            _dbName = "WiresUpServicesStory";
            _conString = GetLocalDbConString(_dbName);

            using (var dbContext = new TestModel(_conString))
            {
                dbContext.Contacts.Add(entity);
                dbContext.SaveChanges();
            }
        }

        protected void AndGivenAnSqlDrivenApp()
        {
            using (var dbContext = new TestModel(_conString))
            {
                Assert.True(dbContext.Contacts.Any());
            }

            var cb = new ContainerBuilder();

            cb.RegisterModule(new GenericDataServiceModule());
            cb.RegisterModule(new SqlDataModule
            {
                ConnectionString = _conString
            });

            cb.RegisterAll(new AppTypeScanner(Environment.CurrentDirectory, "FutureState.Autofac"));
            
            cb.Register(m => new KeyGenerator<Contact, int>(() => contactId++))
                .AsSelf().AsImplementedInterfaces().SingleInstance();

            _container = cb.Build();
        }

        protected void WhenResolvingAProviderForAGivenEntity()
        {
            var provider = _container.Resolve<ProviderLinq<Contact, int>>();
            _items = provider.GetAll();
        }

        protected void AndWhenResolvingEntitiesFromUnPopulatedTable()
        {
            var provider = _container.Resolve<ProviderLinq<Address, Guid>>();
            _itemsFromGuidKeyedEntity = provider.GetAll();
        }

        protected void ThenShouldBeAbleToQueryItemsFromTheProvider()
        {
            Assert.Single(_items, m => m.Name == "Name");
            Assert.Empty(_itemsFromGuidKeyedEntity);
        }

        [BddfyFact]
        public void WiresUpServices()
        {
            this.BDDfy();
        }
    }
}