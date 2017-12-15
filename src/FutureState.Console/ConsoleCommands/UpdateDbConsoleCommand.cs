using Autofac;
using FutureState.App;
using FutureState.Data.Autofac.Services;
using FutureState.Data.Keys;
using FutureState.Db.Setup;
using FutureState.Domain;
using FutureState.Domain.Providers.Autofac;
using FutureState.Domain.Services;
using FutureState.Etl;
using FutureState.Security;
using ManyConsole;
using NLog;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;

namespace FutureState.Console.ConsoleCommands
{
    public class UpdateDbConsoleCommand : ConsoleCommand
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        IContainer _container;

        public string ConnectionString { get; private set; }

        public UpdateDbConsoleCommand()
        {
            this.IsCommand("update-database","Update the default database specified in the application's configuration filee.");

            // ensure database is always up to date
            string conString = ConfigurationManager.ConnectionStrings["FutureStateDb"]?.ConnectionString;

            this.ConnectionString = conString ?? throw new InvalidOperationException("Can't find connection string setting '{FutureStateDb}'.");

            this.HasOption("con=", "Connection string to database.", c => this.ConnectionString = c);
        }

        public override int Run(string[] remainingArguments)
        {
            InitApp();

            UpdateDatase();

            LoadData();

            return 0;
        }

        private void LoadData()
        {
            var loader = this._container.Resolve<BatchEtlLoader>();

            var loadResult = loader.LoadFrom("Data")
                .ToArray();
        }

        public int UpdateDatase()
        {
            if (_logger.IsInfoEnabled)
                _logger.Info($"Dev mode detected. Ensuring database model is up to date.");


            var sqlBuilder = new SqlConnectionStringBuilder(ConnectionString);
            string dbName = sqlBuilder.InitialCatalog;
            string dacPathFile = Environment.CurrentDirectory + @"\FutureState.Db.dacpac";
            if (!File.Exists(dacPathFile))
                throw new InvalidOperationException($"Can't find future state dac path file at: '{dacPathFile}'.");

            var directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);

            directoryInfo = directoryInfo.Parent.Parent;
            // will create / update the database if required
            FsDatabaseInstaller.CreateUpgrade(dbName, directoryInfo.FullName, dacPathFile);

            if (_logger.IsInfoEnabled)
                _logger.Info($"Updagraded database.");

            return 0;
        }

        public void InitApp()
        {
            // ReSharper disable once UnusedVariable
            var appInstance = Application.Instance;

            var cb = new ContainerBuilder();
            cb.RegisterModule<GenericDataServiceModule>();

            cb.BuildApp()
                .RegisterUnitsOfWork()
                .RegisterTypes<IKeyGetter>()
                .RegisterTypes<IEntityIdProvider>()
                .RegisterServices()
                .RegisterEntityTableMaps()
                .RegisterSpecializedQueries()
                .RegisterValidators()
                .RegisterClassMappers();

            // don't register as implemented interfaces
            cb.RegisterGeneric(typeof(FsService<>));

            cb.RegisterAssemblyTypes(typeof(BusinessUnitLoader).Assembly)
                .Where(m => typeof(ILoader).IsAssignableFrom(m))
                .AsSelf()
                .AsImplementedInterfaces();

            cb.RegisterModule(new SqlDataModule
            {
                ConnectionString = this.ConnectionString
            });

            cb.RegisterType<BatchEtlLoader>();

            _container = cb.Build();

            // assign current identities
            Thread.CurrentPrincipal = new FSPrinciple(
                new FSIdentity("Admin", Stakeholder.Admin.Id),
                new[] { "Admins" });

        }
    }
}
