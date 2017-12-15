using NLog;
using System;
using System.Data.SqlClient;
using System.IO;

namespace FutureState.Db.Setup
{
    /// <summary>
    ///     Simplifies the setup of a local ms-sql database on a given host.
    /// </summary>
    public class LocalDbSetup
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // todo: pull from machine config file
        public const string LocalDbServerName = @"(localdb)\MSSQLLocalDB";

        // connection string to local db
        static readonly string connectionString = $@"Data Source={LocalDbServerName};Initial Catalog=master;Integrated Security=True";

        static readonly object _syncLock = new object();
        DbInfo _dbInfo;
        readonly string _dbFileName;
        readonly string _logFileName;
        readonly string _dbName;

        public DbInfo DbInfo => _dbInfo;

        /// <summary>
        ///     Creates a new instance to setup a database in a given directory with a given name.
        /// </summary>
        public LocalDbSetup(string dataBaseDir, string dbName)
        {
            string mdfFilename = dbName + ".mdf";

            this._dbName = dbName;
            this._dbFileName = Path.Combine(dataBaseDir, mdfFilename);
            this._logFileName = Path.Combine(dataBaseDir, $"{dbName}_log.ldf");

            // Create Data Directory If It Doesn't Already Exist.
            if (!Directory.Exists(dataBaseDir))
                Directory.CreateDirectory(dataBaseDir);
        }

        /// <summary>
        ///     Gets or creates a database with the instance's configuration settings.
        /// </summary>
        /// <remarks>
        ///     Will create a database with the assigned database name and file path.
        /// </remarks>
        /// <param name="deleteIfExists">Delete any existing database with the active database name.</param>
        /// <returns></returns>
        public void CreateLocalDb(bool deleteIfExists = false)
        {
            lock (_syncLock)
            {
                // If the file exists, and we want to delete old data, remove it here and create a new database.
                if (File.Exists(_dbFileName) && deleteIfExists)
                {
                    if (File.Exists(_logFileName))
                        File.Delete(_logFileName);

                    File.Delete(_dbFileName);

                    CreateDatabase(_dbName, _dbFileName);
                }
                // If the database does not already exist, create it.
                else if (!File.Exists(_dbFileName))
                {
                    CreateDatabase(_dbName, _dbFileName);
                }

                // ensure db files are not read-only
                File.SetAttributes(_dbFileName, FileAttributes.Normal);
                File.SetAttributes(_logFileName, FileAttributes.Normal);

                this._dbInfo = new DbInfo(_dbName, _dbFileName, _logFileName);
            }
        }

        /// <summary>
        ///     Creates a database with a given name and the given databae file path.
        /// </summary>
        internal static bool CreateDatabase(string dbName, string dbFileName)
        {
            lock (_syncLock)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    SqlCommand cmd = connection.CreateCommand();

                    TryDetachDatabase(dbName);

                    cmd.CommandText = $"CREATE DATABASE [{dbName}] ON (NAME = N'{dbName}', FILENAME = '{dbFileName}')";
                    cmd.ExecuteNonQuery();
                }

                return File.Exists(dbFileName);
            }
        }

        /// <summary>
        ///     Detach the active database.
        /// </summary>
        public bool TryDetachDatabase()
        {
            return TryDetachDatabase(this._dbName);
        }

        /// <summary>
        ///     Detaches a database by a given name.
        /// </summary>
        /// <param name="dbName">The name of the database to detach.</param>
        /// <returns>True if the database has been detached or does not exist.</returns>
        public static bool TryDetachDatabase(string dbName)
        {
            try
            {
                lock (_syncLock)
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // don't detach if it already exists
                        {
                            SqlCommand cmd = connection.CreateCommand();
                            cmd.CommandText = $"select count(*) from sysdatabases where name = '{dbName}'";
                            if ((int) cmd.ExecuteScalar() == 0)
                                return true;
                        }

                        // take offline
                        {
                            SqlCommand cmd = connection.CreateCommand();
                            cmd.CommandText = $"alter database [{dbName}] set offline with rollback immediate";
                            cmd.ExecuteNonQuery();
                        }


                        // detach
                        {
                            SqlCommand cmd = connection.CreateCommand();
                            cmd.CommandText = $"exec sp_detach_db '{dbName}'";
                            cmd.ExecuteNonQuery();
                        }

                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);

                return false;
            }
        }
    }
}