using System.Data;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     A wrapper around <see cref="IDbTransaction"/> transaction.
    /// </summary>
    public class Transacton : ITransaction
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="connection">
        ///     The connection to create the transaction under.
        /// </param>
        internal Transacton(IDbConnection connection)
        {
            UnderlyingTransaction = connection.BeginTransaction();

            IsPending = true;
        }

        /// <summary>
        ///
        /// </summary>
        public IDbTransaction UnderlyingTransaction { get; }

        /// <summary>
        ///     Rolls back any pending transactions and disposes any underlying connection.
        /// </summary>
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