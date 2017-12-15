namespace FutureState.Data
{
    /// <summary>
    /// Creates data store sessions (connections to a data base).
    /// </summary>
    public interface ISessionFactory
    {
        // id may be helpful in identifying connection problems a.n.a
        /// <summary>
        /// A unique instance id.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets the new or existing data store connection or session.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();
    }
}