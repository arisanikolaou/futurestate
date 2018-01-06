#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     An managed connection to a given data store.
    /// </summary>
    public sealed class DataSession : IDisposable
    {
        private readonly Lazy<ISession> _getSession;

        internal DataSession(Lazy<ISession> getSession)
        {
            _getSession = getSession;
        }

        public bool IsDisposed { get; private set; }

        internal ISession Session => _getSession.Value;

        /// <summary>
        ///     Disposes and closes the underlying data session.
        /// </summary>
        public void Dispose()
        {
            Close();

            IsDisposed = true;

            GC.SuppressFinalize(this);
        }

        private void Close()
        {
            if (_getSession.IsValueCreated)
                _getSession.Value.Dispose();
        }

        ~DataSession()
        {
            try
            {
                Close();
            }
            catch
            {
                // ignored
            }
        }
    }
}