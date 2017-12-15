using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Data
{
    /// <summary>
    /// Optimizes read/write operations to a common data store by one or more repositories or readers.
    /// Changes are committed using a configurable commit policy.
    /// </summary>
    /// <remarks>
    /// Units of work are re-useable unless disposed but are not thread safe.
    /// Commit/rollback implementations are controlled through an <see cref="ICommitPolicy" /> instance.
    /// </remarks>
    public class UnitOfWork : DataSessionManager, IUnitOfWork
    {
        readonly ICommitPolicy _commitPolicy;

        internal readonly HashSet<object> _deleted;

        internal readonly Queue<Action> _executionQueue;

        internal readonly HashSet<object> _inserted;

        internal readonly HashSet<object> _modified;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="sessionFactory">The underlying data session factory to use.</param>
        /// <param name="policy">The policy to use when committing changes.</param>
        public UnitOfWork(
            ISessionFactory sessionFactory,
            ICommitPolicy policy = null)
            : base(sessionFactory)
        {
            Guard.ArgumentNotNull(sessionFactory, nameof(sessionFactory));

            _executionQueue = new Queue<Action>();

            _inserted = new HashSet<object>();
            _deleted = new HashSet<object>();
            _modified = new HashSet<object>();

            // do not implement a transaction save commit policy by default
            _commitPolicy = policy ?? new NoOpCommitPolicy();
        }

        /// <summary>
        /// The entities that will be deleted in the next Commit operation.
        /// </summary>
        public object[] Deleted => _deleted.ToArray();

        /// <summary>
        /// The entities that will be inserted in the next Commit operation.
        /// </summary>
        public object[] Inserted => _inserted.ToArray();

        /// <summary>
        /// The entities that will be modified in the next Commit operation.
        /// </summary>
        public object[] Modified => _modified.ToArray();

        /// <summary>
        /// Commits any outstanding changes.
        /// </summary>
        public void Commit()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            OnCommitting();

            var hasPendingChanges = _executionQueue.Count > 0;

            if (hasPendingChanges)
            {
                // ensure open session will throw disposed exception if disposed
                var session = Session;

                try
                {
                    _commitPolicy.OnCommitting(session, _executionQueue.Count, Id);

                    // play back actions
                    while (_executionQueue.Count > 0)
                    {
                        var action = _executionQueue.Dequeue();

                        // execute insert, update or delete
                        action();
                    }

                    // success - call committed
                    _commitPolicy.OnCommitted(Session, Id);

                    OnCommitted();
                }
                catch (Exception ex)
                {
                    _commitPolicy.OnException(Session, ex, Id);

                    throw;
                }
                finally
                {
                    // clear the execution stack and all inserted/updated/deleted objects
                    _executionQueue.Clear();

                    _inserted.Clear();
                    _deleted.Clear();
                    _modified.Clear();
                }
            }
        }

        /// <summary>
        /// Enlists a custom action to be performed whenever the internal change stack
        /// is being executed.
        /// </summary>
        public void Enlist(Action action)
        {
            Guard.ArgumentNotNull(action, nameof(action));

            _executionQueue.Enqueue(action);
        }

        /// <summary>
        /// Gets the number of pending changes.
        /// </summary>
        public int GetPendingChanges()
        {
            return _executionQueue.Count;
        }

        /// <summary>
        /// Gets the display name of the Data access.
        /// </summary>
        public override string ToString()
        {
            return @"UnitOfWork {0} : {1}".Params(GetType().Name, Id ?? GetSessionFactory().ToString());
        }

        /// <summary>
        /// Raised when the Data access has been committed.
        /// </summary>
        protected virtual void OnCommitted()
        {
        }

        /// <summary>
        /// Raised when the Data access is committing.
        /// </summary>
        protected virtual void OnCommitting()
        {
        }

    }
}