using Autofac;
using FutureState.Data.Providers;
using System;

namespace FutureState.Data.Autofac
{
    /// <summary>
    ///     Helps build an autofac container with registrations for a linqreader, repository and unit of work registrations for
    ///     a given
    ///     entity type.
    /// </summary>
    public class DataAccessContainerBuilder<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly ContainerBuilder _containerBuilder;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataAccessContainerBuilder(ContainerBuilder cb)
        {
            _containerBuilder = cb;
        }

        /// <summary>
        ///     Registers a LinqReader in the container for the given entity with the given key type. Linq readers allow an entity to be queried
        /// through the default data acceess provider registered in a given container.
        /// </summary>
        public DataAccessContainerBuilder<TEntity, TKey> RegisterLinqReader()
        {
            _containerBuilder.Register(
                m =>
                {
                    //use same session
                    var sessionFactory = m.Resolve<ISessionFactory>();
                    var ctx = m.Resolve<IComponentContext>();
                    var reader = ctx.GetReader<TEntity, TKey>();

                    return new ProviderLinqReader<TEntity, TKey>(sessionFactory, reader);
                })
                             .As<ProviderLinqReader<TEntity, TKey>>();

            return this;
        }

        /// <summary>
        ///     Registers a linq reader db in the given container.
        /// </summary>
        public DataAccessContainerBuilder<TEntity, TKey> RegisterLinqReaderWith(
            Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
        {
            _containerBuilder.Register(
                m =>
                {
                    var sessionFactory = m.Resolve<ISessionFactory>();

                    return new ProviderLinqReader<TEntity, TKey>(sessionFactory, getLinqReader);
                })
                             .As<ProviderLinqReader<TEntity, TKey>>();

            return this;
        }

        public DataAccessContainerBuilder<TEntity, TKey> RegisterUnitOfWork()
        {
            _containerBuilder.Register(
                m =>
                {
                    var sessionFactory = m.Resolve<ISessionFactory>();
                    var ctx = m.Resolve<IComponentContext>();

                    return new UnitOfWork<TEntity, TKey>(
                        ctx.GetRepository<TEntity, TKey>(),
                        sessionFactory);
                })
                             .As<UnitOfWork<TEntity, TKey>>();
            return this;
        }

        /// <summary>
        ///     Registers a unit of work named after the given data model id.
        /// </summary>
        /// <param name="dbModelId">The partition or data model id.</param>
        /// <returns></returns>
        public DataAccessContainerBuilder<TEntity, TKey> RegisterUnitOfWork(string dbModelId)
        {
            _containerBuilder.Register(
                m =>
                {
                    var sessionFactory = m.ResolveNamed<ISessionFactory>(dbModelId);
                    var ctx = m.Resolve<IComponentContext>();

                    return new UnitOfWork<TEntity, TKey>(ctx.GetRepository<TEntity, TKey>(), sessionFactory);
                })
                             .Named<UnitOfWork<TEntity, TKey>>(dbModelId);

            return this;
        }

        public ContainerBuilder GetContainerBuilder()
        {
            return _containerBuilder;
        }
    }
}