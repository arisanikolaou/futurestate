#region

using System;

#endregion

namespace FutureState
{
    /// <summary>
    ///     Helps manage different application domains.
    /// </summary>
    public static class AppDomainUtil
    {
        /// <summary>
        ///     Creates a simple domain.
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public static AppDomain CreateDomain(string domainName)
        {
            return AppDomain.CreateDomain(
                domainName,
                null,
                GetDefaultAppDomainSetup());
        }

        /// <summary>
        ///     Executes a method in a separate AppDomain.  This should serve as a simple replacement
        ///     of running code in a separate process via a console app.
        /// </summary>
        public static T Run<T>(Func<T> func)
        {
            var domain = CreateDomain($"Delegate Executor {func.GetHashCode()}");

            return domain.Run(func);
        }

        /// <summary>
        ///     Runs a function in a separate application domain.
        /// </summary>
        public static T Run<T>(this AppDomain domain, Func<T> func)
        {
            try
            {
                domain.SetData("toInvoke", func);
                domain.DoCallBack(
                    () =>
                    {
                        var funcToInvoke = AppDomain.CurrentDomain.GetData("toInvoke") as Func<T>;
                        // ReSharper disable once PossibleNullReferenceException
                        AppDomain.CurrentDomain.SetData("result", funcToInvoke());
                    });

                return (T)domain.GetData("result");
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        /// <summary>
        ///     Executes a method in a separate AppDomain.  This should serve as a simple replacement
        ///     of running code in a separate process via a console app.
        /// </summary>
        /// <remarks>
        ///     Type T must be a serializeable or marshal by reference object.
        /// </remarks>
        public static void Run<T>(string domainName, Action<T> action)
        {
            Guard.ArgumentNotNullOrEmpty(domainName, nameof(domainName));

            var domain = AppDomain.CreateDomain(
                domainName,
                AppDomain.CurrentDomain.Evidence,
                GetDefaultAppDomainSetup());

            domain.Run(action);
        }

        /// <summary>
        ///     Executes a method in a separate AppDomain.  This should serve as a simple replacement
        ///     of running code in a separate process via a console app.
        /// </summary>
        public static void Run<T>(this AppDomain domain, Action<T> action)
        {
            var type = typeof(T);

            try
            {
                var instance = (T)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

                action?.Invoke(instance);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        /// <summary>
        ///     Executes a method in a separate AppDomain.  This should serve as a simple replacement
        ///     of running code in a separate process via a console app.
        /// </summary>
        public static void Run(Action func)
        {
            Run(new ActionDelegateWrapper { _func = func }.Invoke);
        }

        private static AppDomainSetup GetDefaultAppDomainSetup()
        {
            return new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                LoaderOptimization = LoaderOptimization.MultiDomainHost,
                PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath
            };
        }

        [Serializable]
        private class ActionDelegateWrapper
        {
            public Action _func;

            public int Invoke()
            {
                _func?.Invoke();

                return 0;
            }
        }
    }
}