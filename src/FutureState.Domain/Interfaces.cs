using System;

namespace FutureState.Domain
{
    /// <summary>
    ///     An asset with a commercial benefit or cost.
    /// </summary>
    public interface IAsset
    {

    }

    /// <summary>
    ///     A record that tracks the user that created/last modified a record.
    /// </summary>
    public interface IAuditable
    {
        string UserName { get; }
        DateTime DateLastModified { get; }
    }
}