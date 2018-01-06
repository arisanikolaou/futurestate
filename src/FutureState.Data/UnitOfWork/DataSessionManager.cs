using System;
using NLog;

namespace FutureState.Data
{
    /// <summary>
    ///     Optimizes access to a given data session by one or more linq readers.
    /// </summary>
    public class DataSessionManager : IDisposable
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ILinqReaderHandler _linqReaderHandler;

        private readonly ISessionFactory _sessionFactory;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="sessionFactory">The underlying session factory to use.</param>
        /// <param name="handler">An optional handler to add responsibilities to a set of linq readers.</param>
        public DataSessionManager(ISessionFactory sessionFactory, ILinqReaderHandler handler = null)
        {
            Guard.ArgumentNotNull(sessionFactory, nameof(sessionFactory));

            _sessionFactory = sessionFactory;
            _linqReaderHandler = handler ?? new NoOpLinqReaderHandler();
        }

        /// <summary>
        ///     Gets the active open session.
        /// </summary>
        internal DataSession ActiveDataSession { get; private set; }

        /// <summary>
        ///     An optional identifier for the instance.
        /// </summary>
        public string Id { get; protected set; }


        internal bool IsClosed => ActiveDataSession == null || ActiveDataSession.IsDisposed;

        /// <summary>
        ///     Gets whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     Always gets an active open session from the underlying session factory.
        /// </summary>
        protected internal ISession Session
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("The unit of work has been disposed.");

                if (ActiveDataSession == null)
                    throw new InvalidOperationException(
                        "Open must be called before the underlying data session can be accessed.");

                if (ActiveDataSession.IsDisposed)
                    throw new InvalidOperationException(
                        "The underlying data session has been disposed and can no longer be used.");

                return ActiveDataSession.Session;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //calls close
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Closes any active data session.
        /// </summary>
        public virtual void Close()
        {
            if (ActiveDataSession != null)
            {
                ActiveDataSession.Dispose();
                ActiveDataSession = null;
            }
        }

        /// <summary>
        ///     Associates a given reader to the current instance to take advantage of session pooling.
        /// </summary>
        public EntitySetReader<TEntity, TKey> GetManagedReader<TEntity, TKey>(
            Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
            where TEntity : class, IEntity<TKey>, new()
        {
            Guard.ArgumentNotNull(getLinqReader, nameof(getLinqReader));

            getLinqReader = GetHandler(getLinqReader);

            var repository = new EntitySetReader<TEntity, TKey>(getLinqReader, this);

            return repository;
        }

        /// <summary>
        ///     Explicitly opens a new disposable data session.
        /// </summary>
        public virtual DataSession Open()
        {
            if (ActiveDataSession != null && !ActiveDataSession.IsDisposed)
                throw new InvalidOperationException(
                    "There already is an active session open. Close must be called before another session can be opened.");

            return ActiveDataSession = new DataSession(new Lazy<ISession>(_sessionFactory.Create));
        }

        // base must be called by derived classes
        public override string ToString()
        {
            return $"Session Manager {GetType().FullName} : {Id ?? _sessionFactory.ToString()}";
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <remarks>
        ///     Will close any underlying data connections.
        /// </remarks>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only.
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                Close();
            }
        }

        /// <summary>
        ///     Gets the handler(s) for a given link reader.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being provided by the reader.</typeparam>
        /// <typeparam name="TKey">They type of key for the given entity.</typeparam>
        /// <param name="getLinqReader">The function to get the 'underlying' reader.</param>
        /// <returns></returns>
        protected virtual Func<ISession, ILinqReader<TEntity, TKey>> GetHandler<TEntity, TKey>(
            Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
            where TEntity : class, IEntity<TKey>, new()
        {
            return _linqReaderHandler.HandleReader(getLinqReader);
        }


        protected ISessionFactory GetSessionFactory()
        {
            return _sessionFactory;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="DataSessionManager" /> class.
        /// </summary>
        ~DataSessionManager()
        {
            // have to assume any open session uses unmanaged resources
            try
            {
                Dispose(false);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // don't bubble exceptions
            }
        }

        private class NoOpLinqReaderHandler : ILinqReaderHandler
        {
            Func<ISession, ILinqReader<TEntity, TKey>> ILinqReaderHandler.HandleReader<TEntity, TKey>(
                Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
            {
                return getLinqReader;
            }
        }
    }
}