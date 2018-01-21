using System;
using System.IO;
using System.Linq;
using CsvHelper;
using FutureState.Flow.Core;
using FutureState.Flow.Tests.Mock;
using FutureState.Specifications;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    public class CanProcessIncomingDataInFlowsToFileAndSqlServerStory
    {
        private const string DataFileToCreate = "CsvProcessorUnitTests-Source.csv";
        private const int CsvItemsToCreate = 10;

        private ProcessResult<DenormalizedEntity, Dto1> _resultA;
        private ProcessResult<Dto1, Dto2> _resultB;
        private SpecProvider<Dto1> _specProvider;

        protected void GivenANewLocalSqlDb()
        {
            using (var db = new TestModel())
            {
                if (db.Database.Exists())
                    db.Database.Delete();

                // save results
                db.Database.CreateIfNotExists();
            }
        }

        protected void AndGivenASetOfSpecifications()
        {
            this._specProvider = new SpecProvider<Dto1>();

            this._specProvider.Add(a =>
            {
                if(a.Source.Key == "Key-5")
                    return new SpecResult("Arbitrary invalid reason.");

                return SpecResult.Success;
            }, "Key", "Description");


            _specProvider.MergeFrom(m => m.Contact, new SpecProvider<Contact>());
        }

        protected void GivenAGeneratedDataSourceCsvFile()
        {
            // always re-create
            if (File.Exists(DataFileToCreate))
                File.Delete(DataFileToCreate);

            using (var fs = File.OpenWrite(DataFileToCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<DenormalizedEntity>();

                    csv.Flush();
                    csv.NextRecord();


                    for (var i = 0; i < CsvItemsToCreate; i++)
                    {
                        var entity = new DenormalizedEntity()
                        {
                            Key = $"Key-{i}",
                            ContactName = $"Contact-{i}",
                            ContactDescription = $"Contact-{i}",
                            Address1 = $"Address-A-{i}",
                            Address2 = $"Address-B-{i}"
                        };

                        csv.WriteRecord(entity);
                        csv.NextRecord();
                    }

                    csv.Flush();
                }
            }
        }

        protected void WhenProcessingADenormalizedFileUsingProcessingRules()
        {
            var processorA = new CsvProcessor<DenormalizedEntity, Dto1>(DataFileToCreate, null, 1, _specProvider)
            {
                BeginProcessingItem = (dtoIn, dtoOut) =>
                {
                    dtoOut.Source = dtoIn;
                    dtoOut.Contact = new Contact()
                    {
                        Id = 0,
                        Name = dtoIn.ContactName,
                        Description = dtoIn.ContactDescription
                    };
                }
            };

            // process result state
            this._resultA = processorA.Process();

            // todo: save to data store
        }

        protected void AndWhenChainingTheProcessedResultsToAnotherProcessor()
        {
            // chain 2
            var processorB = new InMemoryProcessor<Dto1, Dto2>(_resultA.Output)
            {
                BeginProcessingItem = (dtoIn, dtoOut) =>
                {
                    // map object and preserve incoming result
                    dtoOut.Source = dtoIn.Source;
                    dtoOut.Contact = dtoIn.Contact;
                    // don't update FK references or database ids
                    dtoOut.Addresses = new []
                    {
                        new Address()
                        {
                            StreetName = dtoIn.Source.Address1
                        },
                        new Address()
                        {
                            StreetName = dtoIn.Source.Address2
                        }
                    };
                },
                // save to database
                OnCommitting = (processedItems) =>
                {
                    // save to database
                    using (var db = new TestModel())
                    {
                        db.Contacts.AddRange(processedItems.Select(m => m.Contact));

                        // save results to update contact idss
                        db.SaveChanges();
                    }

                    // save addresses now to database and update fk reference obtained above
                    using (var db = new TestModel())
                    {
                        // update mappings and fk references
                        processedItems.Each(m =>
                        {
                            m.Addresses.Each(n => { n.ContactId = m.Contact.Id; });
                        });

                        db.Addresses.AddRange(processedItems.SelectMany(m => m.Addresses));

                        // save results
                        db.SaveChanges();
                    }
                }
            };

            this._resultB = processorB.Process();
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.NotNull(_resultA);
            Assert.NotNull(_resultB);
            Assert.NotNull(_resultB.Output.First().Addresses.First().StreetName);
        }

        protected void AndThenDataShouldBePopulatedWithValidDataOnly()
        {
            Assert.Equal(CsvItemsToCreate, _resultA.ProcessedCount);
            Assert.Equal(CsvItemsToCreate - 1, _resultB.ProcessedCount);
            Assert.Single(_resultA.Errors);

            using (var db = new TestModel())
            {
                // less one as hit the rule
                Assert.Equal(CsvItemsToCreate - 1, db.Contacts.Count());
            }
        }

        [BddfyFact]
        public void CanProcessIncomingDataInFlowsToFileAndSqlServer()
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

            public Address[] Addresses { get; set; }
        }
    }
}
