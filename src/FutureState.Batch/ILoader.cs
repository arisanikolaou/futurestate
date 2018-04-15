namespace FutureState.Batch
{
    /// <summary>
    ///     An ETL loader.
    /// </summary>
    public interface ILoader
    {
        /// <summary>
        ///     The entity type being loaded.
        /// </summary>
        string SchemaTypeCode { get; }

        /// <summary>
        ///     The underlying data store that is being read.
        /// </summary>
        string DataSource { get; set; }

        /// <summary>
        ///     Loads a set of entities into the system.
        /// </summary>
        /// <returns>
        ///     A load result indicating the status of the load transaction.
        /// </returns>
        ILoaderState Load();
    }
}
