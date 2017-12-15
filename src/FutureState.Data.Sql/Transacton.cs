using System.Data;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     A sql transaction.
    /// </summary>
    public class Transacton : ITransaction
    {
        readonly IDbTransaction _transaction;

        public IDbTransaction UnderlyingTransaction => _transaction;

        public void Dispose()
        {
            if (!IsPending) return;

            try
            {
                Rollback();
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        internal Transacton(IDbConnection connection)
        {
            _transaction = connection.BeginTransaction();

            IsPending = true;
        }

        public bool IsPending { get; private set; }

        /// <summary>
        ///     Commits the transaction.
        /// </summary>
        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            finally
            {
                IsPending = false;
            }
        }

        /// <summary>
        ///     Rolls back the transaction and resets the IsPending state.
        /// </summary>
        public void Rollback()
        {
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                IsPending = false;
            }
        }
    }
}