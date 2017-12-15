using FutureState.Data;
using System;
using System.Diagnostics;

namespace FutureState.Data.Providers
{
    /// <summary>
    /// Manages the session required for a given linq reader to operate w/o leaking connections.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class ProviderLinqReader<TEntity, TKey> : DataSessionManager
        where TEntity : class, IEntity<TKey>, new()
    {
        readonly Func<ISession, ILinqReader<TEntity, TKey>> _getLinqReader;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="sessionFactory">The session factory to use the initialize the reader.</param>
        /// <param name="getLinqReader">The function to produce the linq reader.</param>
        public ProviderLinqReader(
            ISessionFactory sessionFactory,
            Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
            : base(sessionFactory)
        {
            _getLinqReader = getLinqReader;
        }

        /// <summary>
        /// Gets the underlying linq reader.
        /// </summary>
        public EntitySetReader<TEntity, TKey> Reader => GetManagedReader(_getLinqReader);

        /// <summary>
        ///     Identifies the session that the current linq reader instance will connect to.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ProviderLinqReader: {GetSessionFactory()}";
        }
    }
}