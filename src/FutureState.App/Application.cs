using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using FutureState.Data;
using FutureState.Data.Sql.Mappings;
using System.IO;
using Dapper;
using NLog;

namespace FutureState.App
{
    //winding up application takes less than a millisecond

    /// <summary>
    ///     Application class used to help windup a given future state application.
    /// </summary>
    public sealed class Application
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // only types associated with this base namespace are processed
        const string AppDomainPrefix = "FutureState";

        static readonly Lazy<Application> _instance;
        readonly Lazy<IList<Type>> _reflectionOnlyTypesGet;
        internal Lazy<IList<IClassMapper>> _classMappersGet;

        internal IList<Type> GetReflectedTypes() { return _reflectionOnlyTypesGet.Value; }
        internal IList<IClassMapper> GetClassMappers() { return _classMappersGet.Value; }

        static Application()
        {
            _instance = new Lazy<Application>(() => new Application());

            // use reflection to scan types quickly
            AppDomain.CurrentDomain
                .ReflectionOnlyAssemblyResolve += (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
        }

        // construct internally
        private Application()
        {
            _reflectionOnlyTypesGet = new Lazy<IList<Type>>(BuildReflectedTypes);
            _classMappersGet = new Lazy<IList<IClassMapper>>(BuildClassMappers);

            // add custom type mappers to help with entity serialization
            SqlMapper.AddTypeHandler(typeof(List<Item>), new JsonListTypeHandler<Item>());
            SqlMapper.AddTypeHandler(typeof(List<string>), new JsonListTypeHandler<string>());
            SqlMapper.AddTypeHandler(typeof(List<Guid>), new JsonListTypeHandler<Guid>());
            SqlMapper.AddTypeHandler(typeof(List<DateTime>), new JsonListTypeHandler<DateTime>());
        }

        /// <summary>
        ///     Gets all public types domain in the application domain.
        /// </summary>
        public IEnumerable<Type> GetAppDomainTypes()
        {
            return _reflectionOnlyTypesGet.Value;
        }

        IList<Type> BuildReflectedTypes()
        {
            // discover by convention all units of work, all queries and all entity maps
            var appDirectory = Assembly.GetCallingAssembly()?.Location;
            var appBinDirectory = new DirectoryInfo(Path.GetDirectoryName(appDirectory));
            var appAssemblies = appBinDirectory.GetFiles(AppDomainPrefix + "*.dll");

            var reflectionOnlyTypes = new List<Type>();

            // use reflection only type discovery to enhance performance
            appAssemblies.Each(m =>
            {
                var reflectionOnlyAssembly = Assembly.ReflectionOnlyLoadFrom(m.FullName);

                // only deal with public types
                reflectionOnlyTypes.AddRange(reflectionOnlyAssembly.GetExportedTypes());
            });

            return reflectionOnlyTypes;
        }

        /// <summary>
        ///     Gets the current active application instance.
        /// </summary>
        public static Application Instance => _instance.Value;


        /// <summary>
        ///     Initialize all default entity mappers to support sql data queries.
        /// </summary>
        IList<IClassMapper> BuildClassMappers()
        {
            // because this is a dynamic type have to compare on type name
            var entityTypes = GetFilteredTypes(
                m => m.GetInterfaces().Any(c =>
                {
                    if (c.Namespace != typeof(IEntity<>).Namespace)
                        return false;

                    return c.Name.StartsWith("IEntity`", StringComparison.OrdinalIgnoreCase);
                }));

            var customClassMappers = GetFilteredTypes(
                m => m.GetInterfaces().Any(c =>
                {
                    if (c.Namespace != typeof(IClassMapper<>).Namespace)
                        return false;
                    if (m.IsAbstract)
                        return false;
                    if (m.IsGenericType)
                        return false;

                    return c.Name.StartsWith("IClassMapper`", StringComparison.OrdinalIgnoreCase);
                }));

            var classMappers = new List<IClassMapper>();

            // assign default fluent mappers for all entity typse
            FluentMapper.Initialize(config =>
            {
                var method = typeof(FluentMapConfiguration).GetMethod("AddMap");

                // add specialized class mapper firs
                foreach (var type in customClassMappers)
                {
                    IClassMapper newCustomEntityMap = Activator.CreateInstance(type.Value) as IClassMapper;

                    var entityProperty = type.Value.GetProperty("EntityType");
                    if (entityProperty == null)
                        throw new InvalidOperationException("Property 'EntityType' does not exist.");

                    Type entityType = entityProperty.GetValue(newCustomEntityMap, new object[0]) as Type;
                    classMappers.Add(newCustomEntityMap);

                    // invoke generic method
                    var generic = method.MakeGenericMethod(entityType);

                    generic.Invoke(config, new[] { newCustomEntityMap });
                }

                // interogate container for entity maps and update
                foreach (var lazyEntityType in entityTypes)
                {
                    var actualType = lazyEntityType.Value;

                    if (actualType == null)
                        continue;

                    if (FluentMapper.EntityMaps.ContainsKey(actualType))
                        continue;

                    try
                    {
                        var newCustomEntityMapType = typeof(CustomEntityMap<>).MakeGenericType(actualType);

                        var newCustomEntityMap = Activator.CreateInstance(newCustomEntityMapType) as IClassMapper;
                        classMappers.Add(newCustomEntityMap);

                        // invoke generic method
                        var generic = method.MakeGenericMethod(actualType);

                        generic.Invoke(config, new object[] { newCustomEntityMap });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to assign default class mapper for {actualType.FullName}",
                            ex);
                    }
                }
            });

            return classMappers;
        }


        /// <summary>
        ///     Gets a filtered list of public types that are not abstract that belong to the application
        ///     scope.
        /// </summary>
        /// <param name="filter">Additional filtering criteria.</param>
        /// <returns></returns>
        public static IList<Lazy<Type>> GetFilteredTypes(Func<Type, bool> filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            IList<Type> reflectedTypes = Instance.GetReflectedTypes();

            var returnTypes = new List<Lazy<Type>>();

            //these are reflection only types
            var filteredTypes = reflectedTypes
                .Where(n => !n.IsAbstract) // always avoid abstract classes
                .Where(n => !string.IsNullOrWhiteSpace(n.AssemblyQualifiedName)) // ensure all have assembly name
                .Where(filter)
                .ToList();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filteredType in filteredTypes)
            {
                // ReSharper disable once AssignNullToNotNullAttribute

                var realType = new Lazy<Type>(() =>
                {
                    var type = Type.GetType(filteredType.AssemblyQualifiedName, true);

                    return type;
                });

                returnTypes.Add(realType);
            }

            return returnTypes;
        }
    }

}