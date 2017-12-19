using System;
using System.Collections.Generic;
using Autofac;
using FutureState.Data;
using FutureState.Data.KeyBinders;
using FutureState.Data.Keys;
using FutureState.Data.Providers;

namespace FutureState.Autofac.Modules
{
    /// <summary>
    ///     Registers in memory repositories, readers and units of work.
    /// </summary>
    public class InMemoryDataAccessModule : Module
    {
        private readonly IList<Action<ContainerBuilder>> _builderConfigurators = new List<Action<ContainerBuilder>>();

        /// <summary>
        ///     Registers functions to resolve a given in memory irepository, ireader and ilinqreader implementation for a given
        ///     entity type and entity type key.
        /// </summary>
        public InMemoryDataAccessModule RegisterRepository<TEntity, TKey>(IEntityIdProvider<TEntity, TKey> idGenerator)
            where TEntity : class, new()
        {
            //register repository functions
            Action<ContainerBuilder> buildGetRepositoryFunction = cb =>
            {
                cb.Register(
                        (m, q) =>
                        {
                            var cc = m.Resolve<IComponentContext>();

                            var fun = cc.GetRepository(idGenerator);

                            return fun;
                        })
                    .As(typeof(Func<ISession, IRepository<TEntity, TKey>>))
                    .SingleInstance();
            };

            _builderConfigurators.Add(buildGetRepositoryFunction);

            //register reader - this has to be a different registration than linq reader or repository implementations
            buildGetRepositoryFunction = cb =>
            {
                cb.Register(
                        (m, q) =>
                        {
                            var cc = m.Resolve<IComponentContext>();

                            var fun = cc.GetReader(idGenerator);

                            return fun;
                        })
                    .As(typeof(Func<ISession, IReader<TEntity, TKey>>))
                    .SingleInstance();
            };

            _builderConfigurators.Add(buildGetRepositoryFunction);

            //register linq reader
            buildGetRepositoryFunction = cb =>
            {
                cb.Register(
                        (m, q) =>
                        {
                            var cc = m.Resolve<IComponentContext>();

                            var fun = cc.GetReader(idGenerator);

                            return fun;
                        })
                    .As(typeof(Func<ISession, ILinqReader<TEntity, TKey>>))
                    .SingleInstance();
            };

            _builderConfigurators.Add(buildGetRepositoryFunction);

            #region overwrite open generic repositories with an implementaiton that relies on the given id generator

            //build irepository resolution functions
            buildGetRepositoryFunction = cb =>
            {
                cb.Register(
                        (m, q) =>
                            new InMemoryRepository<TEntity, TKey>(idGenerator, new AttributeKeyBinder<TEntity, TKey>(),
                                new TEntity[0]))
                    .AsSelf()
                    .As<IRepositoryLinq<TEntity, TKey>>()
                    .As<IGetter<TEntity, TKey>>()
                    .As<IReader<TEntity, TKey>>()
                    .As<IRepository<TEntity, TKey>>()
                    .As<ILinqReader<TEntity, TKey>>()
                    .As<IReader<TEntity, TKey>>()
                    .As<IBulkLinqReader<TEntity, TKey>>()
                    .SingleInstance();
            };

            _builderConfigurators.Add(buildGetRepositoryFunction);

            #endregion overwrite open generic repositories with an implementaiton that relies on the given id generator

            return this;
        }


        /// <summary>
        ///     Registers a single instance in memory repository for any given entity type and an in memory session into a given
        ///     container.
        /// </summary>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(m => new InMemoryRepositoryFactory())
                .As<IRepositoryFactory>();

            builder.Register(m => new NoOpCommitPolicy())
                .As<ICommitPolicy>();

            //units of work
            builder.RegisterGeneric(typeof(UnitOfWork<,>))
                .Named("Default", typeof(UnitOfWork<,>))
                .SingleInstance()
                .AsSelf();

            builder.RegisterGeneric(typeof(UnitOfWorkLinq<,>))
                .Named("Default", typeof(UnitOfWorkLinq<,>))
                .SingleInstance()
                .AsSelf();

            builder.RegisterGeneric(typeof(ProviderLinq<,>))
                .AsSelf()
                .SingleInstance();

            builder.Register(m => new InMemorySession())
                .As<ISession>()
                .SingleInstance();

            builder.Register(m => new InMemorySessionFactory())
                .As<ISessionFactory>()
                .SingleInstance();

            builder.RegisterGeneric(typeof(EntityIdProvider<,>))
                .As(typeof(IEntityIdProvider<,>));

            //in memory repositories
            builder.RegisterGeneric(typeof(InMemoryRepository<>))
                .AsSelf()
                .As(typeof(IRepository<>))
                .As(typeof(IRepositoryLinq<>))
                .As(typeof(ILinqReader<>))
                .As(typeof(IReader<>))
                .Named("Default", typeof(IRepository<>))
                .Named("Default", typeof(IRepositoryLinq<>))
                .Named("Default", typeof(ILinqReader<>))
                .SingleInstance();

            builder.RegisterGeneric(typeof(InMemoryRepository<,>))
                .AsSelf()
                .As(typeof(IRepositoryLinq<,>))
                .As(typeof(IRepository<,>))
                .As(typeof(ILinqReader<,>))
                .Named("Default", typeof(IRepository<,>))
                .Named("Default", typeof(IRepositoryLinq<,>))
                .Named("Default", typeof(ILinqReader<,>))
                .SingleInstance();

            //execute all deferred builder actions after default generic types have been registered.
            _builderConfigurators.Each(builderConfigurator => builderConfigurator(builder));

            _builderConfigurators.Clear();
        }
    }
}