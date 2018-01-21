
namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data received from a csv file.
    /// </summary>
    /// <typeparam name="TEntityIn">The type of entity to read in from the underlying data source.</typeparam>
    /// <typeparam name="TEntityOut">The type of entity that will be produced after processing.</typeparam>
    public class CsvProcessor<TEntityIn, TEntityOut> : Processor<TEntityIn, TEntityOut>
        where TEntityOut : class, new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CsvProcessor(
            string dataSource,
            ProcessorConfiguration<TEntityIn, TEntityOut> configuration = null,
            string processorName = null) : base(
                () => new CsvProcessorReader<TEntityIn>(dataSource).Read(),
                configuration,
                processorName)
        {
            DataSource = dataSource;
        }

        /// <summary>
        ///     Gets or sets the file path to read data from.
        /// </summary>
        public string DataSource { get; }
    }
}