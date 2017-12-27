namespace FutureState.Data
{
    /// <summary>
    ///     Creates data store sessions (connections to a data base).
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        ///     A unique instance id to identify the session.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     Creates a new data store connection.
        /// </summary>
        ISession Create();
    }
}