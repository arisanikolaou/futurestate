#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     An abstraction over a long running data store connection.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        ///     Gets whether the underlying connection to the data store is open or not.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        ///     Begins a net new transaction against the given data store. If the active connection to the store
        ///     is not opened this will automatically open it.
        /// </summary>
        ITransaction BeginTran();

        /// <summary>
        ///     Gets the active transaction object against the data store.
        /// </summary>
        ITransaction GetCurrentTran();
    }
}