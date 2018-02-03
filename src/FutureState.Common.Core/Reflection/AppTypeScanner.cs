using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;

namespace FutureState.Reflection
{
    //winding up application takes less than a millisecond
    /// <summary>
    ///     Fast/safe app domain type scanning.
    /// </summary>
    public class AppTypeScanner
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<AppTypeScanner> _default = new Lazy<AppTypeScanner>(() => new AppTypeScanner());
        private readonly string _basePath;
        private readonly Lazy<IList<Type>> _reflectionOnlyTypesGet;

        static AppTypeScanner()
        {
            // use reflection to scan types quickly
            AppDomain.CurrentDomain
                .ReflectionOnlyAssemblyResolve += (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="assemblyProbePath">Defaults to current directory.</param>
        /// <param name="assemblyScanPrefix">The prefix of the assemblies to filter out.</param>
        public AppTypeScanner(
            string assemblyProbePath = null, string assemblyScanPrefix = "FutureState")
        {
            _basePath = assemblyProbePath ?? Environment.CurrentDirectory;
            _reflectionOnlyTypesGet = new Lazy<IList<Type>>(BuildReflectedTypes);

            AssemblyFilterPrefix = assemblyScanPrefix;
        }

        // only types associated with this base namespace are processed
        /// <summary>
        ///     Gets the assembly prefixes to filter type scanning results to.
        /// </summary>
        public string AssemblyFilterPrefix { get; }

        /// <summary>
        ///     Gets the default app type scanner.
        /// </summary>
        public static AppTypeScanner Default => _default.Value;

        /// <summary>
        ///     Gets all the reflected types.
        /// </summary>
        /// <returns></returns>
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
                appBinDirectory =
                    new DirectoryInfo(Path.GetDirectoryName(appDirectory) ?? throw new InvalidOperationException());
            }
            else
            {
                appBinDirectory = new DirectoryInfo(_basePath);
            }

            // gets all the assemblies
            var appAssemblies = appBinDirectory.GetFiles(AssemblyFilterPrefix + "*.dll");

            var reflectionOnlyTypes = new List<Type>();

            // use reflection only type discovery to enhance performance
            appAssemblies.Each(m =>
            {
                // scan assemblies on a best effort basis
                try
                {
                    var reflectionOnlyAssembly = Assembly.ReflectionOnlyLoadFrom(m.FullName);

                    // only deal with public types
                    reflectionOnlyTypes.AddRange(reflectionOnlyAssembly.GetExportedTypes());
                }
                catch (Exception ex)
                {
                    if (_logger.IsErrorEnabled)
                        _logger.Error(ex, $"Can't scan assembly {m.Name} due to an unexpected error.");
                }
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