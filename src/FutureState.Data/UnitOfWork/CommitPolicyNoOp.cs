#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     A commit policy that relies on a unit of work to commit changes to a data store
    ///     rather than the combination of transactions against a session and the unit of work.
    /// </summary>
    /// <remarks>
    ///     To be unsed in units of work where blocking transactions against a database
    ///     are not desirable.
    /// </remarks>
    public class CommitPolicyNoOp : ICommitPolicy
    {
        public void OnCommitted(ISession session, string id)
        {
            // commit a transaction
        }

        public void OnCommitting(ISession session, int pendingChanges, string id)
        {
        }

        public void OnException(ISession session, Exception ex, string id)
        {
            // exception raising a transaction and rollback
        }
    }
}