using System.Data;

namespace Dapper.Extensions.Linq.Core.Sessions
{
    public class DapperSession : IDapperSession
    {
        public DapperSession(IDbConnection connection)
        {
            Connection = connection;
        }

        public IDbConnection Connection { get; }

        public IDbTransaction Transaction { get; set; }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            Transaction = Connection.BeginTransaction(il);
            return Transaction;
        }

        public IDbTransaction BeginTransaction()
        {
            Transaction = Connection.BeginTransaction();
            return Transaction;
        }

        public void ChangeDatabase(string databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            Connection.Close();
        }

        public string ConnectionString
        {
            get => Connection.ConnectionString;
            set => Connection.ConnectionString = value;
        }

        public int ConnectionTimeout => Connection.ConnectionTimeout;

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public string Database => Connection.Database;

        public void Open()
        {
            Connection.Open();
        }

        public ConnectionState State => Connection.State;

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}