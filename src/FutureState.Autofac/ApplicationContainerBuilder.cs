using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Autofac;
using Dapper.Extensions.Linq.Core.Mapper;
using FutureState.Data;
using FutureState.Data.Sql;
using FutureState.Reflection;
using FutureState.Services;
using FutureState.Specifications;
using Magnum.Reflection;
using NLog;

namespace FutureState.Autofac
{
    /// <summary>
    ///     Helps construct a given autofac container using the types discovered through a given <see cref="AppTypeScanner" />
    ///     instance.
    /// </summary>
    public class ApplicationContainerBuilder
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly ContainerBuilder _cb;
        private readonly AppTypeScanner _scanner;

        public ApplicationContainerBuilder(ContainerBuilder cb, AppTypeScanner scanner)
        {
            _cb = cb ?? throw new ArgumentNullException(nameof(cb));
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        public ApplicationContainerBuilder RegisterAll()
        {
            return RegisterValidators()
                .RegisterServices()
                .RegisterEntityTableMaps()
                .RegisterSpecializedQueries()
                .RegisterUnitsOfWork()
                .RegisterEntityKeyGenerators()
                .RegisterClassMappers();
        }

        private ApplicationContainerBuilder RegisterEntityKeyGenerators()
        {
            foreach (var entity in _scanner.GetTypes<IEntity>())
            {
                var entityType = entity.Value;

                var pk = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.GetCustomAttributes<KeyAttribute>(true).Any());

                if (pk == null)
                    continue;

                if (pk.PropertyType == typeof(Guid))
                    this.FastInvoke(new[]
                        {
                            entityType,
                            pk.PropertyType
                        },
                        "RegisterEntityKeyGenerator", new Func<Guid>(SeqGuid.Create));
            }

            return this;
        }

        protected void RegisterEntityKeyGenerator<TEntity, TKey>(Func<TKey> getKey)
        {
            _cb.Register(m => new KeyGenerator<TEntity, TKey>(getKey))
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        /// <summary>
        ///     Registers all classes that inherit from IService.
        /// </summary>
        private ApplicationContainerBuilder RegisterServices()
        {
            return RegisterTypes<IService>();
        }

        /// <summary>
        ///     Registers all classes that implement a class mapper.
        /// </summary>
        private ApplicationContainerBuilder RegisterClassMappers()
        {
            var provider = new AppClassMapProvider(_scanner);

            foreach (var classMapper in provider.GetClassMappers())
                _cb.Register(m => classMapper)
                    .AsSelf()
                    .As<IClassMapper>()
                    .PreserveExistingDefaults();


            return this;
        }

        /// <summary>
        ///     Registers all units of work discovered in the application space within a given autofac
        ///     container builder.
        /// </summary>
        private ApplicationContainerBuilder RegisterUnitsOfWork()
        {
            // have to compare by type name as this is reflection only type
            var unitOfWorkTypes = _scanner.GetFilteredTypes(
                m => m.GetInterfaces().Any(c => c.FullName == typeof(IUnitOfWork).FullName));

            foreach (var unitOfWork in unitOfWorkTypes)
            {
                var type = unitOfWork.Value;

                var registration = _cb.RegisterType(type)
                    .AsSelf()
                    .InstancePerLifetimeScope() // allow db to be shared by multiple dependencies
                    .PreserveExistingDefaults();

                // require special handling which is beyond me
                foreach (var interfaceType in type.GetInterfaces())
                    if (interfaceType.AssemblyQualifiedName != null)
                        registration.As(interfaceType);
                    else if (_logger.IsDebugEnabled)
                        _logger.Debug($"Type has invalid interface map: {type.FullName}");
            }

            return this;
        }

        /// <summary>
        ///     Register all specification providers/validators for a given entity type.
        /// </summary>
        /// <returns></returns>
        private ApplicationContainerBuilder RegisterValidators()
        {
            return RegisterTypes<IProvideSpecifications>();
        }


        private ApplicationContainerBuilder RegisterEntityTableMaps()
        {
            return RegisterTypes<IClassMapper>();
        }

        /// <summary>
        ///     Registers all types in all assemblies in the application scope that implement a given interface.
        /// </summary>
        /// <typeparam name="TInterfaceType">The interface to match.</typeparam>
        /// <returns></returns>
        public ApplicationContainerBuilder RegisterTypes<TInterfaceType>()
        {
            // will return all non abstract public types
            var lazies = _scanner.GetTypes<TInterfaceType>();

            foreach (var lazy in lazies)
                _cb.RegisterType(lazy.Value)
                    .AsSelf()
                    .AsImplementedInterfaces();

            return this;
        }

        /// <summary>
        ///     Registers all specialized data queries discovered in the application space within a given
        ///     autofac container builder.
        /// </summary>
        private ApplicationContainerBuilder RegisterSpecializedQueries()
        {
            // get queries that don't have any generic arguments
            // types that are abstract are already excluded
            var dataQueryTypesLazy = _scanner.GetFilteredTypes(
                    m => m.GetInterfaces().Any(c => c.FullName == typeof(IDataQuery).FullName))
                .Where(m => !m.Value.GetGenericArguments().Any());

            var method = typeof(ApplicationContainerBuilder).GetMethod("RegisterDataQuery");
            if (method == null)
                throw new InvalidOperationException(
                    $"Method 'RegisterDataQuery' not found on class: {typeof(ApplicationContainerBuilder).FullName}");

            foreach (var dataQueryLazy in dataQueryTypesLazy)
            {
                _cb.RegisterType(dataQueryLazy.Value)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .PreserveExistingDefaults();

                // register self
                var genericMethod = method.MakeGenericMethod(dataQueryLazy.Value);
                genericMethod.Invoke(this, new object[] { });

                // register on each interface type deriving from IDataQuery
                var specializedQueryInterfaces =
                    dataQueryLazy.Value.GetInterfaces()
                        .Where(inf => inf != typeof(IDataQuery) && typeof(IDataQuery).IsAssignableFrom(inf));

                foreach (var specializedDataQueryInterface in specializedQueryInterfaces)
                {
                    genericMethod = method.MakeGenericMethod(specializedDataQueryInterface);

                    genericMethod.Invoke(this, new object[] { });
                }
            }

            return this;
        }

        // called via reflection - keep this public
        public ApplicationContainerBuilder RegisterDataQuery<TDataQuery>()
            where TDataQuery : IDataQuery
        {
            _cb.Register(m =>
                {
                    var cntx = m.Resolve<IComponentContext>();

                    var func =
                        new Func<ISession, TDataQuery>(
                            session => cntx.Resolve<TDataQuery>(new TypedParameter(typeof(ISession), session)));

                    return func;
                }).As<Func<ISession, TDataQuery>>()
                .PreserveExistingDefaults();

            return this;
        }
    }
}