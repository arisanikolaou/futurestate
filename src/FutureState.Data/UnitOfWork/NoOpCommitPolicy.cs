#region

using System;

#endregion

namespace FutureState.Data
{
    // should be stateless
    public class NoOpCommitPolicy : ICommitPolicy
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