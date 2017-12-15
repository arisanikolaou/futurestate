#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// Controls how a unit of work will commit changes to an underlying data store
    /// given certain system states.
    /// </summary>
    public interface ICommitPolicy
    {
        void OnCommitted(ISession session, string id);

        void OnCommitting(ISession session, int pendingChanges, string id);

        void OnException(ISession session, Exception ex, string id);
    }
}