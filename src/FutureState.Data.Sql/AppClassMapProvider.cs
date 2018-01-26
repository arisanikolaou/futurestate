using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using FutureState.Data.Sql.Mappings;
using FutureState.Reflection;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     Helps construct the class maps required to serializer/deserialize
    /// entities from an underlying data store.
    /// </summary>
    public class AppClassMapProvider
    {
        private readonly AppTypeScanner _scanner;

        static AppClassMapProvider()
        {
            // add custom type mappers to help with entity serialization
            SqlMapper.AddTypeHandler(typeof(List<Item>), new JsonListTypeHandler<Item>());
            SqlMapper.AddTypeHandler(typeof(List<string>), new JsonListTypeHandler<string>());
            SqlMapper.AddTypeHandler(typeof(List<Guid>), new JsonListTypeHandler<Guid>());
            SqlMapper.AddTypeHandler(typeof(List<DateTime>), new JsonListTypeHandler<DateTime>());
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="scanner">The assembly type scanner to use.</param>
        public AppClassMapProvider(AppTypeScanner scanner)
        {
            _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        }

        /// <summary>
        ///     Initialize all default entity mappers to support sql data queries
        /// that can be discovered in the assemblies referenced by the current instance.
        /// </summary>
        public IList<IClassMapper> GetClassMappers()
        {
            // because this is a dynamic type have to compare on type name
            IList<Lazy<Type>> entityTypes = _scanner.GetTypes<IEntity>().ToList();

            IList<Lazy<Type>> customClassMappers = _scanner.GetTypes<IClassMapper>().ToList();

            var classMappers = new List<IClassMapper>();

            // assign default fluent mappers for all entity typse
            FluentMapper.Initialize(config =>
            {
                var method = typeof(FluentMapConfiguration).GetMethod("AddMap");
                if (method == null)
                    throw new InvalidOperationException("AddMap method not found.");

                // add specialized class mapper firs
                foreach (var type in customClassMappers)
                {
                    var newCustomEntityMap = Activator.CreateInstance(type.Value) as IClassMapper;

                    var entityProperty = type.Value.GetProperty("EntityType");
                    if (entityProperty == null)
                        throw new InvalidOperationException("Property 'EntityType' does not exist.");

                    Type entityType = entityProperty.GetValue(newCustomEntityMap, new object[0]) as Type;
                    classMappers.Add(newCustomEntityMap);

                    // invoke generic method
                    // ReSharper disable once PossibleNullReferenceException
                    MethodInfo generic = method.MakeGenericMethod(entityType);

                    generic.Invoke(config, new object[] { newCustomEntityMap });
                }

                // interogate container for entity maps and update
                foreach (Lazy<Type> lazyEntityType in entityTypes)
                {
                    var actualType = lazyEntityType.Value;

                    if (actualType == null)
                        continue;

                    if (FluentMapper.EntityMaps.ContainsKey(actualType))
                        continue;

                    try
                    {
                        var newCustomEntityMapType = typeof(CustomEntityMap<>)
                            .MakeGenericType(actualType);

                        // create class mapper instance
                        var newCustomEntityMap = Activator.CreateInstance(newCustomEntityMapType) as IClassMapper;
                        classMappers.Add(newCustomEntityMap);

                        // invoke generic method
                        // ReSharper disable once PossibleNullReferenceException
                        var generic = method.MakeGenericMethod(actualType);

                        try
                        {
                            generic.Invoke(config, new object[] { newCustomEntityMap });
                        }
                        catch (TargetInvocationException tex)
                        {
                            if (tex.InnerException != null)
                            {
                                if (!tex.InnerException.Message.Contains("The type already exists."))
                                    throw;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Failed to assign default class mapper for {actualType.FullName} due to an unexpected error.",
                            ex);
                    }
                }
            });

            return classMappers;
        }
    }
}