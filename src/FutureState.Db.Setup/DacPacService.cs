using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Dac;

namespace FutureState.Db.Setup
{
    /// <summary>
    ///     Simplifies setting up a sql server database via a dacpac file.
    /// </summary>
    public class DacPacService
    {
        public static readonly string LocalDbServerName = LocalDbSetup.LocalDbServerName;

        public DacPacService()
        {
            Messages = new List<string>();
        }

        /// <summary>
        ///     List of messages received from the underlying dac pac service.
        /// </summary>
        public List<string> Messages { get; }

        /// <summary>
        ///     Sets up a datatabase from a given dacpac file.
        /// </summary>
        /// <param name="databaseName">The name of the databaes to install the database to.</param>
        /// <param name="dacPacFilePath"></param>
        public void Deploy(string databaseName, string dacPacFilePath)
        {
            if (!File.Exists(dacPacFilePath))
                throw new ArgumentOutOfRangeException(dacPacFilePath);

            // deploy to a local sql server
            var conBuilder = new SqlConnectionStringBuilder
            {
                DataSource = LocalDbServerName,
                InitialCatalog = "master"
            };

            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            var conString = conBuilder.ToString();

            Deploy(conString, databaseName, dacPacFilePath);
        }

        /// <summary>
        ///     Deploys a given database from a dacpac file.
        /// </summary>
        public void Deploy(string connectionString,
            string databaseName,
            string dacPacFileName)
        {
            Messages.Add($"Deploying database: {databaseName}");

            var dacOptions = new DacDeployOptions
            {
                BlockOnPossibleDataLoss = false,
                TreatVerificationErrorsAsWarnings = true,
                AllowIncompatiblePlatform = true,
                IgnoreFileAndLogFilePath = true
            };

            var dacServiceInstance = new DacServices(connectionString);

            dacServiceInstance.ProgressChanged += (s, e) => Messages.Add(e.Message);
            dacServiceInstance.Message += (s, e) => Messages.Add(e.Message.Message);

            try
            {
                using (var dacpac = DacPackage.Load(dacPacFileName))
                {
                    dacServiceInstance.Deploy(dacpac, databaseName,
                        true, // upgrade existing
                        dacOptions);
                }
            }
            catch (Exception ex)
            {
                Messages.Add(ex.Message);

                throw;
            }
        }
    }
}