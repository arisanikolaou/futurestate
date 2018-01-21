using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using FutureState.Flow.Core;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    public class CsvProcessorUnitTests
    {
        string dataFile = "DataFile.csv";
        private List<Dto1> _processedItems;
        private ProcessResult _processBResult;
        private List<Dto2> _processBItems;
        private ProcessResult _processResult;

        protected void GivenAGeneratedDataSourceCsvFile()
        {
            if (File.Exists(dataFile))
                File.Delete(dataFile);

            using (var fs = File.OpenWrite(dataFile))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<DenormalizedEntity>();

                    csv.Flush();
                    csv.NextRecord();

                    for (var i = 0; i < 50; i++)
                    {
                        var entity = new DenormalizedEntity()
                        {
                            Key = $"Key{i}",
                            ContactName = $"Contact{i}",
                            ContactDescription = $"Contact{i}",
                            Address1 = $"Address1{i}",
                            Address2 = $"Address2{i}"
                        };

                        csv.WriteRecord(entity);
                        csv.NextRecord();
                    }

                    csv.Flush();
                }
            }
        }

        protected void WhenProcessingADenormalizedFile()
        {
            var processorA = new CsvProcessor<DenormalizedEntity, Dto1>(dataFile)
            {
                BeginProcessingItem = (dto1, dto2) =>
                {
                    dto2.Source = dto1;
                    dto2.Contact = new Contact()
                    {
                        Id = 0,
                        Name = dto1.ContactName,
                        Description = dto1.ContactDescription
                    };
                },
                WorkingFolder = $@"{Environment.CurrentDirectory}\ProcessedResults"
            };

            // process result state
            this._processResult = processorA.Process();
            // processed items
            this._processedItems = processorA.ProcessedItems;

            // todo: save to data store
        }

        protected void AndWhenChainingTheProcessedResultsToAnotherProcessor()
        {
            // chain
            var processorB = new InMemoryProcessor<Dto1, Dto2>(_processedItems)
            {
                BeginProcessingItem = (dto1, dto2) =>
                {
                    // map object
                    dto2.Source = dto1.Source;
                    dto2.Contact = dto1.Contact;
                    dto2.Address = new[]
                    {
                        new Address()
                        {
                            Id = 0,
                            StressName = dto1.Source.Address1
                        },
                        new Address()
                        {
                            Id = 0,
                            StressName = dto1.Source.Address2
                        }
                    };
                },
                WorkingFolder = $@"{Environment.CurrentDirectory}\ProcessedResults"
            };


            this._processBResult = processorB.Process();
            this._processBItems = processorB.ProcessedItems;
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.NotNull(_processResult);

            Assert.NotNull(_processBResult);

            Assert.Equal(50, _processBResult.ProcessedCount);
            Assert.NotNull(_processBItems.First().Address.First().StressName);
        }

        [BddfyFact]
        public void ProcessedDenormalizedResultsInAChain()
        {
            this.BDDfy();
        }

        public class DenormalizedEntity
        {
            public string Key { get; set; }

            public string ContactName { get; set; }

            public string ContactDescription { get; set; }

            public string Address1 { get; set; }

            public string Address2 { get; set; }
        }

        public class Dto1
        {
            public DenormalizedEntity Source { get; set; }

            public Contact Contact { get; set; }
        }

        public class Dto2
        {
            public DenormalizedEntity Source { get; set; }

            public Contact Contact { get; set; }

            public Address[] Address { get; set; }
        }

        public class Contact
        {
            public int Id { get; set; }

            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }

            public string StressName { get; set; }
        }
    }
}
