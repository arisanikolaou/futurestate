using System;
using Autofac;
using FutureState.Autofac.Modules;
using FutureState.Data;
using FutureState.Reflection;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Autofac.Tests
{
    [Story]
    public class InMemoryRepositorySupportsIdInsertsStory
    {
        private IContainer _container;
        private Contact _entity;
        private Contact _entity2;
        private InMemoryRepository<Contact, int> _provider;

        protected void GivenAnInMemoryDrivenApp()
        {
            var cb = new ContainerBuilder();

            cb.RegisterModule(new GenericDataAccessModule());
            cb.RegisterAll(
                new AppTypeScanner(Environment.CurrentDirectory,
                "FutureState.Autofac"));
            // register after
            cb.RegisterModule(new InMemoryDataAccessModule());

            _container = cb.Build();
        }

        protected void AndGivenARepository()
        {
            this._provider = _container.Resolve<InMemoryRepository<Contact, int>>();
        }

        protected void WhenAddingAnEntitiesToInMemoryRepos()
        {
            var entity = new Contact
            {
                Id = 0,
                Name = "Name"
            };

            _provider.Insert(entity);

            _entity = entity;

            var entity2 = new Contact
            {
                Id = 0,
                Name = "Name 2"
            };

            _provider.Insert(entity2);
            _entity2 = entity2;
        }

        protected void TheneEntityIdsShouldBeAssigned()
        {
            Assert.Equal(1, _entity.Id);
            Assert.Equal(2, _entity2.Id);
        }

        [BddfyFact]
        public void InMemoryRepositorySupportsInsert()
        {
            this.BDDfy();
        }
    }
}
