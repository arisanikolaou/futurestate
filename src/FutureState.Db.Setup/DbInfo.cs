﻿namespace FutureState.Db.Setup
{
    /// <summary>
    ///     Basic setup/configuration properties of a sql server.
    /// </summary>
    public sealed class DbInfo
    {
        public DbInfo(string dbName, string dbFileName, string logFileName)
        {
            DbName = dbName;
            DbFileName = dbFileName;
            LogFileName = logFileName;
        }

        public string LogFileName { get; }

        public string DbFileName { get; }

        public string DbName { get; }
    }
}