
namespace FutureState.Flow.Core
{
    public abstract class CsvProcessorBase<TEntityIn, TEntityOut> : ProcessorSingleResult<TEntityIn, TEntityOut>
        where TEntityOut : class , new()
    {

        public string DataSource { get; }

        protected CsvProcessorBase(string dataSource) : base(() => new CsvProcesorReader<TEntityIn>(dataSource).Read())
        {
            DataSource = dataSource;
        }

    }
}