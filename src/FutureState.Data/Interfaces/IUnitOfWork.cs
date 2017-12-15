#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// Implements the unit of work pattern that groups operations to update a database in one transaction.
    /// </summary>
    /// <remarks>
    /// Consumers must explicitly open/close (or dispose) the object to interact with any data provided by it.
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Closes/resets the state of the Data access.
        /// </summary>
        void Close();

        /// <summary>
        /// Commits any outstanding changes.
        /// </summary>
        void Commit();

        /// <summary>
        /// Begins a disposable read only 'Data access data session'.
        /// </summary>
        /// <remarks>
        /// Sessions should always be disposed which will cause any underlying connections to close.
        /// </remarks>
        DataSession Open();
    }
}