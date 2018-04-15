using Autofac;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace FutureState.Batch.Tests
{
    [Story]
    public class LoadsBasicBatchDataWithCsvStory : LoadsBasicBatchDataBaseStory
    {
        protected override string GetDataSource()
        {
            return @"Input\DataSource.csv";
        }

        protected override void BuildLoader(ContainerBuilder builder)
        {
            // return sample contact
            builder.RegisterType<MaybeLoader>();

            builder
                .RegisterType<CsvExtractor<MaybeLoaderDto>>()
                .As<IExtractor<MaybeLoaderDto>>();
        }

        [BddfyFact]
        public void LoadsBasicBatchDataWithCsv()
        {
            this.BDDfy();
        }
    }
}
