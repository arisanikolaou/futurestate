using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FutureState.Reflection
{
    //winding up application takes less than a millisecond

    public class AppTypeScanner
    {
        private readonly Lazy<IList<Type>> _reflectionOnlyTypesGet;


        private readonly string _basePath;

        static AppTypeScanner()
        {
            // use reflection to scan types quickly
            AppDomain.CurrentDomain
                .ReflectionOnlyAssemblyResolve += (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
        }

        // construct internally
        public AppTypeScanner(string assemblyProbePath = null, string assemblyScanPrefix = null)
        {
            _basePath = assemblyProbePath;
            _reflectionOnlyTypesGet = new Lazy<IList<Type>>(BuildReflectedTypes);

            AssemblyFilterPrefix = assemblyScanPrefix ?? "FutureState";
        }

        // only types associated with this base namespace are processed
        public string AssemblyFilterPrefix { get; }

        internal IList<Type> GetReflectedTypes()
        {
            return _reflectionOnlyTypesGet.Value;
        }

        /// <summary>
        ///     Gets all public types domain in the application domain.
        /// </summary>
        public IEnumerable<Type> GetAppDomainTypes()
        {
            return _reflectionOnlyTypesGet.Value;
        }

        private IList<Type> BuildReflectedTypes()
        {
            // discover by convention all units of work, all queries and all entity maps
            DirectoryInfo appBinDirectory;
            if (_basePath == null)
            {
                var appDirectory = Assembly.GetCallingAssembly()?.Location;
                appBinDirectory = new DirectoryInfo(Path.GetDirectoryName(appDirectory) ?? throw new InvalidOperationException());
            }
            else
            {
                appBinDirectory = new DirectoryInfo(_basePath);
            }

            var appAssemblies = appBinDirectory.GetFiles(AssemblyFilterPrefix + "*.dll");

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
        ///     Gets all types implementing a given interface.
        /// </summary>
        public IEnumerable<Lazy<Type>> GetTypes<TInterfaceType>()
        {
            // will return all non abstract public types
            var lazies = GetFilteredTypes(
                    m => m.IsClass
                         && !m.IsAbstract
                         && m.GetInterfaces().Any(c => c.FullName == typeof(TInterfaceType).FullName))
                .Where(m => !m.Value.GetGenericArguments().Any());

            return lazies;
        }

        /// <summary>
            ///     Gets a filtered list of public types that are not abstract that belong to the application
            ///     scope.
            /// </summary>
            /// <param name="filter">Additional filtering criteria.</param>
            /// <returns></returns>
            public IList<Lazy<Type>> GetFilteredTypes(Func<Type, bool> filter)
        {
            Guard.ArgumentNotNull(filter, nameof(filter));

            var reflectedTypes = GetReflectedTypes();

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