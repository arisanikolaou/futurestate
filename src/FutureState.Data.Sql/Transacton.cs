using System.Data;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     A sql transaction.
    /// </summary>
    public class Transacton : ITransaction
    {
        internal Transacton(IDbConnection connection)
        {
            UnderlyingTransaction = connection.BeginTransaction();

            IsPending = true;
        }

        public IDbTransaction UnderlyingTransaction { get; }

        public void Dispose()
        {
            if (!IsPending) return;

            try
            {
                Rollback();
            }
            finally
            {
                UnderlyingTransaction.Dispose();
            }
        }

        public bool IsPending { get; private set; }

        /// <summary>
        ///     Commits the transaction.
        /// </summary>
        public void Commit()
        {
            try
            {
                UnderlyingTransaction.Commit();
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
                UnderlyingTransaction.Rollback();
            }
            finally
            {
                IsPending = false;
            }
        }
    }
}