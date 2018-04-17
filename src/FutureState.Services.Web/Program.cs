using System.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System;
using NLog;

namespace FutureState.Services.Web
{
    public class Program
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var assembly = typeof(Program).Assembly;
            if (_logger.IsInfoEnabled)
                _logger.Info($"Starting FutureState Web Services. Assembly Version {assembly.GetName().Version}.");

            // configure logging
            var loggingFactory = new LoggerFactory();

            string hostDirectory = Directory.GetCurrentDirectory();
            string env = ConfigurationManager.AppSettings["EnvironmentName"];

            string webServerUrl = ConfigurationManager.AppSettings["WebServerUrl"];

            if (string.IsNullOrWhiteSpace(webServerUrl))
                _logger.Error($"Configuration setting {webServerUrl} has not been set.");

            IWebHost host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(hostDirectory)
                .ConfigureLogging(factory =>
                {
                    factory
                        .AddConsole()
                        .AddDebug();
                })
                .CaptureStartupErrors(true)
                .UseEnvironment(env) // enviroment code
                .UseUrls(webServerUrl) // startup path
                .UseStartup<Startup>()
                .Build();

            // always update
            bool isDevBuild = env == "Development";

            if (_logger.IsInfoEnabled)
                _logger.Info($"Starting web server on: {webServerUrl}. Dev mode enabled: {isDevBuild}.");

            if (!isDevBuild)
            {
                host.Run();
            }
            else
            {
                // will be different from current directory
                string assemblyPath = Path.GetDirectoryName(assembly.Location);
                RunDevModel(assemblyPath, host);
            }
        }

        static void RunDevModel(string hostDirectory, IWebHost host)
        {
            if (_logger.IsInfoEnabled)
                _logger.Info($"Dev mode detected. Ensuring database model is up to date.");

            // will create / update the database if required
            try
            {
                host.Run();
            }
            finally
            {
                if (_logger.IsInfoEnabled)
                    _logger.Info($"Shutting down.");
            }
        }
    }

    // application default exception handler
}
