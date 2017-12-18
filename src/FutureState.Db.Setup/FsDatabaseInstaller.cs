using System;
using System.Data.SqlClient;
using System.IO;
using NLog;

namespace FutureState.Db.Setup
{
    public class FsDatabaseInstaller
    {
        private const string modelDbName = "FSModel";
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static LocalDbSetup _modelDbSetup;

        /// <summary>
        ///     Sets up a 'model' FutureState database to be copied from.
        /// </summary>
        public static void CreateModel(string deploymentDir, string dacPacFile = "FutureState.Db.dacpac")
        {
            _modelDbSetup = CreateUpgrade(modelDbName, deploymentDir, dacPacFile);

            // detatch database so that it can be copied
            LocalDbSetup.TryDetachDatabase(modelDbName);
        }

        /// <summary>
        ///     Creates and/or updates a given database.
        /// </summary>
        public static LocalDbSetup CreateUpgrade(string dbName, string deploymentDir,
            string dacPacFile = "FutureState.Db.dacpac")
        {
            var lastWriteTime = File.GetLastWriteTimeUtc(dacPacFile).ToFileTime();

            deploymentDir = $@"{deploymentDir}\Db-{dbName}-{lastWriteTime}";

            var dbSetup = new LocalDbSetup(deploymentDir, dbName);

            if (!Directory.Exists(deploymentDir) || Directory.GetFiles(deploymentDir).Length == 0)
            {
                _logger.Info("{0}{1}", "Creating model database from: ", Path.GetFullPath(dacPacFile));
                Directory.CreateDirectory(deploymentDir);

                // ensure not read only
                var dirInfo = new DirectoryInfo(deploymentDir)
                {
                    Attributes = FileAttributes.Normal
                };

                // create database
                dbSetup.CreateLocalDb();

                // upgrade/setup model database
                var dacPac = new DacPacService();
                dacPac.Deploy(dbName, dacPacFile);
            }
            else
            {
                // attach existing database
                dbSetup.CreateLocalDb();

                _logger.Debug("Model database has already been created.");
                // otherwise model database is the same, re-creating is expensive, assume database has already bee created
            }

            return dbSetup;
        }

        /// <summary>
        ///     Creates a database copy of the FutureState database.
        /// </summary>
        public DbInfo CreateDb(string dataDir = null, string dbName = "TestFSDb")
        {
            var deploymentDir = dataDir ?? Environment.CurrentDirectory;

            if (_modelDbSetup == null)
                CreateModel(deploymentDir); // create automagically

            var deploymentDirInfo = new DirectoryInfo(deploymentDir);
            //remove read only attribute
            deploymentDirInfo.Attributes &= ~FileAttributes.ReadOnly;

            LocalDbSetup.TryDetachDatabase(dbName);

            var fileName = $@"{deploymentDir}\{dbName}.mdf";
            var logFileName = $@"{deploymentDir}\{dbName}_log.ldf";


            // ReSharper disable once PossibleNullReferenceException
            File.Copy(_modelDbSetup.DbInfo.DbFileName, fileName, true);
            File.Copy(_modelDbSetup.DbInfo.LogFileName, logFileName, true);

            var attributes = File.GetAttributes(fileName);
            var logAttributes = File.GetAttributes(logFileName);

            File.SetAttributes(fileName, FileAttributes.Normal); // overwrite read-only attributes
            File.SetAttributes(logFileName, FileAttributes.Normal); // overwrite read-only attribute

            var dbConString =
                $@"Data Source={LocalDbSetup.LocalDbServerName};AttachDBFileName={fileName};Initial Catalog={
                        dbName
                    };Integrated Security=True;";

            // test connection
            using (var con = new SqlConnection(dbConString))
            {
                con.Open();

                // ensure read/write
                var sql = $"ALTER DATABASE [{dbName}] SET READ_WRITE WITH NO_WAIT";

                var com = new SqlCommand(sql, con);
                com.ExecuteNonQuery();
            }

            return new DbInfo(dbName, fileName, logFileName);
        }

        /// <summary>
        ///     Creates a databaes copy of the FutureState database.
        /// </summary>
        public DbInfo CreateDbIfNotExists(string dataDir = null, string dbName = "TestFSDb")
        {
            var deploymentDir = dataDir ?? Environment.CurrentDirectory;

            var fileName = $@"{deploymentDir}\{dbName}.mdf";
            var logFileName = $@"{deploymentDir}\{dbName}_log.ldf";

            // re-use existing connection
            if (File.Exists(fileName) && File.Exists(logFileName))
                return new DbInfo(dbName, fileName, logFileName);

            return CreateDb(dataDir, dbName);
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}