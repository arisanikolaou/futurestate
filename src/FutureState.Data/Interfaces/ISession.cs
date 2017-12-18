#region

using System;

#endregion

namespace FutureState.Data
{
    // an - todo: may consider renaming to data session

    /// <summary>
    ///     ISession abstracts a connection to a given data store.
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
        /// <returns></returns>
        ITransaction BeginTran();

        /// <summary>
        ///     Gets the active transaction object against the database.
        /// </summary>
        /// <returns></returns>
        ITransaction GetCurrentTran();
    }
}