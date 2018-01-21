namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data received from a csv file.
    /// </summary>
    /// <typeparam name="TEntityIn">The type of entity to read in from the underlying data source.</typeparam>
    /// <typeparam name="TEntityOut">The type of entity that will be produced after processing.</typeparam>
    public class CsvProcessor<TEntityIn, TEntityOut> : ProcessorSingleResult<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="dataSource"></param>
        public CsvProcessor(string dataSource) : base(() => new CsvProcessorReader<TEntityIn>(dataSource).Read())
        {
            DataSource = dataSource;
        }

        /// <summary>
        ///     Gets or sets the file path to read data from.
        /// </summary>
        public string DataSource { get; }
    }
}