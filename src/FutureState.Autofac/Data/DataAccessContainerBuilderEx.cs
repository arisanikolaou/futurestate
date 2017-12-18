using Autofac;
using FutureState.Data;

namespace FutureState.Autofac.Data
{
    /// <summary>
    ///     Extension methods to autofac's container builder.
    /// </summary>
    public static class DataAccessContainerBuilderEx
    {
        /// <summary>
        ///     Registers a generic linq reader for a given entity and its key in the container.
        /// </summary>
        public static DataAccessContainerBuilder<TEntity, TKey> RegisterLinqReader<TEntity, TKey>(
            this ContainerBuilder builder)
            where TEntity : class, IEntity<TKey>, new()
        {
            var containerBuilder = new DataAccessContainerBuilder<TEntity, TKey>(builder);

            containerBuilder.RegisterLinqReader();

            return containerBuilder;
        }

        /// <summary>
        ///     Registers a new unit of work for a given entity and its key type in the container.
        /// </summary>
        public static DataAccessContainerBuilder<TEntity, TKey>
            RegisterUoW<TEntity, TKey>(this ContainerBuilder builder) where TEntity : class, IEntity<TKey>, new()
        {
            var containerBuilder = new DataAccessContainerBuilder<TEntity, TKey>(builder);

            containerBuilder.RegisterUnitOfWork();

            return containerBuilder;
        }

        /// <summary>
        ///     Registers a unit of work into the given container associated with a given partition id.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name">The data model or partition id.</param>
        /// <returns></returns>
        public static DataAccessContainerBuilder<TEntity, TKey> RegisterUoW<TEntity, TKey>(
            this ContainerBuilder builder, string name) where TEntity : class, IEntity<TKey>, new()
        {
            var containerBuilder = new DataAccessContainerBuilder<TEntity, TKey>(builder);

            containerBuilder.RegisterUnitOfWork(name);

            return containerBuilder;
        }
    }
}