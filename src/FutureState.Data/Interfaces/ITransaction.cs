#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// Represents a data store transaction. All transactions should be committed before they are disposed otherwise
    /// they will be rolled back.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Gets whether a transaction is pending meaning that it neither has been committed or rolled back.
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// Commits a transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back a transaction. If the transaction is disposed and a transaction is pending
        /// the transaction will be rolled back.
        /// </summary>
        void Rollback();
    }
}