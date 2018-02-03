using CsvHelper;
using FutureState.Flow.Data;
using FutureState.Flow.Tests.Mock;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    [Collection("Flow Tests")]
    public class CanProcessIncomingDataInFlowsToFileAndSqlServerStory
    {
        private const string DataFileToCreate = "CsvProcessorUnitTests-Source.csv";
        private const int CsvItemsToCreate = 10;

        private readonly Guid _processId = Guid.Parse("523a8558-e5a5-4309-ad20-f3813997e651");
        private const int BatchId = 1;

        private ProcessResult<DenormalizedEntity, Dto1> _resultA;
        private ProcessResult<Dto1, Dto2> _resultB;
        private SpecProvider<Dto1> _specProvider;
        private Processor<DenormalizedEntity, Dto1> _processorA;
        private BatchProcess _batchProcess;
        private ProcessResult<Dto2, Address> _resultC;
        private SpecProvider<Address> _specProviderFroAddress;
        private ProcessResultRepository<ProcessResult> _repository;

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

        protected void AndGivenAProcessorResultsRepository()
        {
            _repository = new ProcessResultRepository<ProcessResult>(Environment.CurrentDirectory);
        }

        protected void AndGivenAbatchProcess()
        {
            _batchProcess = new BatchProcess(_processId, BatchId);
        }

        protected void AndGivenASetOfSpecificationsForSource()
        {
            _specProvider = new SpecProvider<Dto1>();

            _specProvider.Add(a =>
            {
                if (a.Source.Key == "Key-5")
                    return new SpecResult("Arbitrary invalid reason.");

                return SpecResult.Success;
            }, "Key", "Description");

            _specProvider.MergeFrom(m => m.Contact, new SpecProvider<Contact>());
        }

        protected void AndGivenASetOfSpecificationsForAddresses()
        {
            _specProviderFroAddress = new SpecProvider<Address>();

            _specProviderFroAddress.Add(a =>
            {
                if (a.ContactId == 0)
                    return new SpecResult("Contact Id has not been assigned.");

                return SpecResult.Success;
            }, "Key", "Description");
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
            var processorA = new Processor<DenormalizedEntity, Dto1>(
                new ProcessorConfiguration<DenormalizedEntity, Dto1>(_specProvider,
                new SpecProvider<IEnumerable<Dto1>>()),
                new ProcessorEngine<DenormalizedEntity>())
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

            _processorA = processorA;

            // process from csv
            var enumeration = new CsvProcessorReader<DenormalizedEntity>().Read(DataFileToCreate);
            _resultA = processorA.Process(enumeration, _batchProcess);

            _repository.Save(_resultA);

            // todo: save to data store
        }

        protected void AndWhenChainingTheProcessedResultsToAnotherProcessor()
        {
            // chain 2
            var processorB = new Processor<Dto1, Dto2>(
                new ProcessorConfiguration<Dto1, Dto2>(
                    new SpecProvider<Dto2>(),
                    new SpecProvider<IEnumerable<Dto2>>()),
                new ProcessorEngine<Dto1>())
            {
                BeginProcessingItem = (dtoIn, dtoOut) =>
                {
                    // map object and preserve incoming result
                    dtoOut.Source = dtoIn.Source;
                    dtoOut.Contact = dtoIn.Contact;
                    // don't update FK references or database ids
                    dtoOut.Addresses = new[]
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
                OnCommitting = processedItems =>
                {
                    // save contacts to database
                    using (var db = new TestModel())
                    {
                        // ReSharper disable once PossibleMultipleEnumeration
                        db.Contacts.AddRange(processedItems.Select(m => m.Contact));

                        // save results to update contact idss
                        db.SaveChanges();

                        // update mappings and fk references
                        processedItems.Each(m =>
                        {
                            m.Addresses.Each(n => { n.ContactId = m.Contact.Id; });
                        });
                    }
                }
            };

            _resultB = processorB.Process(_resultA.Output, _batchProcess);

            _repository.Save(_resultB);
        }

        protected void AndWhenChainingTheProcessedResultsToLastProcessor()
        {
            // chain 2
            var processorC = new Processor<Dto2, Address>(
                new ProcessorConfiguration<Dto2, Address>(
                    _specProviderFroAddress,
                    new SpecProvider<IEnumerable<Address>>()),
                new ProcessorEngine<Dto2>())
            {
                CreateOutput = (dtoIn) => dtoIn.Addresses,
                // save to database
                OnCommitting = processedItems =>
                {
                    // save addresses now to database and update fk reference obtained above
                    using (var db = new TestModel())
                    {
                        db.Addresses.AddRange(processedItems);

                        // save results
                        db.SaveChanges();
                    }
                }
            };

            _resultC = processorC.Process(_resultB.Output, _batchProcess);
            _repository.Save(_resultC);
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.NotNull(_resultA);
            Assert.NotNull(_resultB);
            Assert.NotNull(_resultB.Output.First().Addresses.First().StreetName);
        }

        protected void AndThenAllResultsShouldBeProcessedAndOnlyValidContactsShouldBeSaved()
        {
            Assert.Equal(CsvItemsToCreate - 1, _resultA.ProcessedCount);
            Assert.Equal(CsvItemsToCreate - 1, _resultB.ProcessedCount);
            Assert.Single(_resultA.Errors);

            using (var db = new TestModel())
            {
                // less one as hit the rule
                Assert.Equal(CsvItemsToCreate - 1, db.Contacts.Count());
            }
        }

        protected void AndThenOnlyAddressWithValidContactsShouldBeInserted()
        {
            using (var db = new TestModel())
            {
                // less one as hit the rule
                Assert.Equal(CsvItemsToCreate * 2 - 2, db.Addresses.Count());
            }
            Assert.Equal(CsvItemsToCreate - 1, _resultC.ProcessedCount);
        }

        protected void AndThenShouldBeAbleToRestoreProcessState()
        {
            var repo = new ProcessResultRepository<ProcessResult<DenormalizedEntity, Dto1>>(Environment.CurrentDirectory);
            var processorName = Processor<DenormalizedEntity, Dto1>.GetProcessName(_processorA);

            var result = repo.Get(processorName, _processId, BatchId);

            Assert.NotNull(result);
            Assert.Equal(CsvItemsToCreate - 1, result.ProcessedCount);
            Assert.Single(result.Errors);
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