using System;

namespace FutureState.Data
{
    /// <summary>
    ///     A policy of relying on an underlying data session to 
    /// manage datat store transactions. This is the default commit policy.
    /// </summary>
    public class CommitPolicy : ICommitPolicy
    {
        /// <summary>
        ///     Commits any active transactions associated with a given session.
        /// </summary>
        /// <param name="session">The session to evaluate.</param>
        /// <param name="id"></param>
        public void OnCommitted(ISession session, string id)
        {
            // don't guard input to avoid taxing performance
            var tran = session.GetCurrentTran();

            tran?.Commit();
        }

        /// <summary>
        ///     Relies on the session to create a database transaction if and only if pending changes
        ///     is greater than 1.
        /// </summary>
        /// <param name="session">The underlying session.</param>
        /// <param name="pendingChanges">The number of pending actions to make.</param>
        /// <param name="id">The transaction identifier.</param>
        public void OnCommitting(ISession session, int pendingChanges, string id)
        {
            // don't guard input to avoid taxing performance
            if (pendingChanges > 0)
                session.BeginTran();
        }

        /// <summary>
        ///     Executes any actions appropriate to respond to general exceptions
        ///     committing values to an underlying data store.
        /// </summary>
        public void OnException(ISession session, Exception ex, string id)
        {
            var tran = session.GetCurrentTran();
            tran?.Rollback();
        }
    }
}