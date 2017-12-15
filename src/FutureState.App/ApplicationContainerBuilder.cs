using System;
using System.Linq;
using Autofac;
using Dapper.Extensions.Linq.Core.Mapper;
using FutureState.Data;
using NLog;
using FutureState.Services;
using FutureState.Specifications;

namespace FutureState.App
{
    public class ApplicationContainerBuilder
    {
        readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly ContainerBuilder cb;

        public ApplicationContainerBuilder(ContainerBuilder cb)
        {
            Guard.ArgumentNotNull(cb, nameof(cb));

            this.cb = cb;
        }

        /// <summary>
        ///     Registers all classes that inherit from IService.
        /// </summary>
        public ApplicationContainerBuilder RegisterServices()
        {
            return RegisterTypes<IService>();
        }

        /// <summary>
        ///     Registers all classes that implement a class mapper.
        /// </summary>
        /// <returns></returns>
        public ApplicationContainerBuilder RegisterClassMappers()
        {
            foreach (var classMapper in Application.Instance.GetClassMappers())
                cb.Register(m => classMapper)
                    .AsSelf()
                    .As<IClassMapper>()
                    .PreserveExistingDefaults();


            return this;
        }

        /// <summary>
        ///     Registers all units of work discovered in the application space within a given autofac
        /// container builder.
        /// </summary>
        public ApplicationContainerBuilder RegisterUnitsOfWork()
        {
            // have to compare by type name as this is reflection only type
            var unitOfWorkTypes = Application.GetFilteredTypes(
                m => m.GetInterfaces().Any(c => c.FullName == typeof(IUnitOfWork).FullName));

            foreach (var unitOfWork in unitOfWorkTypes)
            {
                var type = unitOfWork.Value;

                var registration = cb.RegisterType(type)
                    .AsSelf()
                    .InstancePerLifetimeScope() // allow db to be shared by multiple dependencies
                    .PreserveExistingDefaults();

                // require special handling which is beyond me
                foreach (var interfaceType in type.GetInterfaces())
                    if (interfaceType.AssemblyQualifiedName != null)
                        registration.As(interfaceType);
                    else
                        if(_logger.IsDebugEnabled)
                            _logger.Debug($"Type has invalid interface map: {type.FullName}");
            }

            return this;
        }

        /// <summary>
        ///     Register all specification providers.
        /// </summary>
        /// <returns></returns>
        public ApplicationContainerBuilder RegisterValidators()
        {
            return RegisterTypes<IProvideSpecifications>();
        }


        public ApplicationContainerBuilder RegisterEntityTableMaps()
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
            var dataQueryTypesLazy = Application.GetFilteredTypes(
                    m => m.IsClass && !m.IsAbstract && m.GetInterfaces().Any(c => c.FullName == typeof(TInterfaceType).FullName))
                    .Where(m => !m.Value.GetGenericArguments().Any())
                    .ToCollection();

            foreach (var dataQueryLazy in dataQueryTypesLazy)
            {
                cb.RegisterType(dataQueryLazy.Value)
                    .AsSelf()
                    .AsImplementedInterfaces();
            }

            return this;
        }

        /// <summary>
        ///     Registers all specialized data queries discovered in the application space within a given
        /// autofac container builder.
        /// </summary>
        public ApplicationContainerBuilder RegisterSpecializedQueries()
        {
            // get queries that don't have any generic arguments
            // types that are abstract are already excluded
            var dataQueryTypesLazy = Application.GetFilteredTypes(
                    m => m.GetInterfaces().Any(c => c.FullName == typeof(IDataQuery).FullName))
                .Where(m => !m.Value.GetGenericArguments().Any());

            var method = typeof(ApplicationContainerBuilder).GetMethod("RegisterDataQuery");
            if (method == null)
                throw new InvalidOperationException($"Method 'RegisterDataQuery' not found on class: {typeof(ApplicationContainerBuilder).FullName}");

            foreach (var dataQueryLazy in dataQueryTypesLazy)
            {
                cb.RegisterType(dataQueryLazy.Value)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .PreserveExistingDefaults();

                // register self
                var genericMethod = method.MakeGenericMethod(dataQueryLazy.Value);
                genericMethod.Invoke(this, new object[0] {  });

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
            cb.Register(m =>
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
