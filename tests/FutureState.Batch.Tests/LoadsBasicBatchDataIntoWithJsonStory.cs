using Autofac;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace FutureState.Batch.Tests
{
    [Story]
    public class LoadsBasicBatchDataIntoWithJsonStory : LoadsBasicBatchDataBaseStory
    {
        protected override string GetDataSource()
        {
            return @"Input\DataSource.json";
        }
        
        protected override void BuildLoader(ContainerBuilder builder)
        {
            // return sample contact
            builder.RegisterType<MaybeLoader>();

            builder.Register(m =>
            {
                return new JsonExtractor<MaybeLoaderDto>();
            })
            .As<IExtractor<MaybeLoaderDto>>();
        }

        [BddfyFact]
        public void LoadsBasicBatchDataIntoWithJson()
        {
            this.BDDfy();
        }
    }
}
