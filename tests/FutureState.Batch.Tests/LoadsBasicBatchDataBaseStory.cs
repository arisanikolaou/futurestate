using Autofac;
using FutureState.Autofac.Modules;
using FutureState.Data;
using FutureState.Data.Providers;
using System.IO;
using System.Linq;
using Xunit;

namespace FutureState.Batch.Tests
{
    public abstract class LoadsBasicBatchDataBaseStory
    {
        protected IContainer _container;
        protected MaybeLoader _loader;
        protected ILoaderState _state;

        protected abstract string GetDataSource();

        protected void GivenACsvFileWithValidAndInvalidData()
        {
            if (!File.Exists(GetDataSource()))
                throw new InvalidDataException("Data source does not exist.");
        }

        protected void AndGivenATestContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterModule(new GenericDataAccessModule());

            // post data to an in memory repository
            builder.RegisterModule(new InMemoryDataAccessModule());

            int globalContactsKey = 0;
            // key provider for contacts
            builder.Register(m =>
            {
                return new KeyGenerator<MaybeEntity, int>(() => ++globalContactsKey);
            }).As<IKeyGenerator<MaybeEntity, int>>().SingleInstance(); // ensure single instance

            BuildLoader(builder);

            // build container
            _container = builder.Build();
        }

        protected abstract void BuildLoader(ContainerBuilder builder);

        protected void AndGivenAWiredUpContactsLoaderWithMaxBatchSize()
        {
            // create loader
            _loader = _container.Resolve<MaybeLoader>();
            _loader.MaxBatchSize = 5; // should produce 2 batches
        }

        protected void WhenLoading()
        {
            _loader.DataSource = GetDataSource();

            _state = _loader.Load();
        }

        protected void ThenValidDataShouldBeLoaded()
        {
            var provider = _container.Resolve<ProviderLinq<MaybeEntity, int>>();

            MaybeEntity[] entities = provider.GetAll().ToArray();

            Assert.Equal(6, entities.Count());

            // ensure data has been populated
            Assert.NotNull(entities.FirstOrDefault(m => m.FirstName == "John"));
            Assert.DoesNotContain(entities, m => m.Id == 0); // ensure all have valid keys
        }

        protected void AndThenLoadStateShouldHaveErrorsRecorded()
        {
            Assert.NotNull(_state);
            Assert.True(0 != _state.ErrorsCount);
        }

        protected void AndThenEntitiesShouldBeProcessedInBatches()
        {
            Assert.Equal(2, _state.Batches);
        }
    }
}
