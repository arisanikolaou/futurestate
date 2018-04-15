using Autofac;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using System;

namespace FutureState.Batch.Tests
{
    [Story]
    public class LoadsBasicBatchDataIntoWithXmlStory : LoadsBasicBatchDataBaseStory
    {
        protected override string GetDataSource()
        {
            return @"Input\DataSource.xml";
        }
        
        protected override void BuildLoader(ContainerBuilder builder)
        {
            // return sample contact
            builder.RegisterType<MaybeLoader>();

            builder.Register(m =>
            {
                return new XmlAttributeEntityExtractor<MaybeLoaderDto>()
                {
                    SelectionNode = "entity"
                };
            })
            .As<IExtractor<MaybeLoaderDto>>();
        }

        [BddfyFact]
        public void LoadsBasicBatchDataIntoWithXml()
        {
            this.BDDfy();
        }
    }
}
