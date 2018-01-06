using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Basic setup/configuration properties of a sql server.
    /// </summary>
    public sealed class DbInfo
    {
        public DbInfo(string dbName, string dbFileName, string logFileName)
        {
            DbName = dbName ?? throw new ArgumentNullException(nameof(dbName));
            DbFileName = dbFileName ?? throw new ArgumentNullException(nameof(dbFileName));
            LogFileName = logFileName ?? throw new ArgumentNullException(nameof(logFileName));
        }

        public string LogFileName { get; }

        public string DbFileName { get; }

        public string DbName { get; }
    }
}