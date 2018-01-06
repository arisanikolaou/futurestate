#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Implements the unit of work pattern that groups operations to read/write to a data store in one transaction.
    /// </summary>
    /// <remarks>
    ///     Consumers must explicitly open/close (or dispose) the object to interact with any data provided by it.
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        ///     Commits any outstanding changes.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Begins a disposable read only data session within which reads/writes should be executed.
        /// </summary>
        /// <remarks>
        ///     Sessions should always be disposed after a unit of work is committed or disposed of by the caller.
        /// </remarks>
        DataSession Open();
    }
}