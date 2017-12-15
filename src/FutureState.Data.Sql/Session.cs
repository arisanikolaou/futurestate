using System.Data;
using System.Data.SqlClient;
using Dapper.Extensions.Linq.Core.Configuration;

namespace FutureState.Data.Sql
{ 
    /// <summary>
    ///     A managed sql connection.
    /// </summary>
    public class Session : ISession
    {
        readonly SqlConnection _connection;
        Transacton _transction;

        public IDapperConfiguration Configuration { get; }

        public bool IsOpen => _connection.State == ConnectionState.Open;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="config"></param>
        internal Session(SqlConnection connection, IDapperConfiguration config)
        {
            _connection = connection;

            // TODO: durable open
            _connection.Open();

            Configuration = config;
        }

        /// <summary>
        ///     Gets the underlying sql connection.
        /// </summary>
        public SqlConnection GetConnection() => _connection;

        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <returns></returns>
        public ITransaction BeginTran()
        {
            _transction = new Transacton(_connection);

            return _transction;
        }

        /// <summary>
        ///     Dispose the instance.
        /// </summary>
        public void Dispose()
        {
            _connection?.Dispose();
            _transction?.Dispose();
        }

        /// <summary>
        ///     Gets the active transaction or null if no transaction has been started.
        /// </summary>
        /// <returns></returns>
        public ITransaction GetCurrentTran()
        {
            return _transction;
        }
    }
}