﻿using Autofac;
using CsvHelper;
using FutureState.Flow.Data;
using FutureState.Flow.Tests.Mock;
using FutureState.Specifications;
using System;
using System.IO;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;
using F = FutureState.Flow;

namespace FutureState.Flow.Tests
{
    [Story]
    [Collection("Flow Tests")]
    public class CanProcessIncomingDataInFlowsToFileAndSqlServerWithAutofacStory
    {
        private const string DataFileToCreate = "CsvProcessorUnitTests-Source.csv";
        private const int CsvItemsToCreate = 10;
        private const int BatchId = 1;
        private readonly F.FlowId _flow = new F.FlowId("TestFlow2");
        private FlowBatch _batchProcess;
        private ContainerBuilder _cb;
        private IContainer _container;
        private Processor<DenormalizedEntity, Dto1> _processorA;
        private FlowSnapshotRepo<FlowSnapshot> _repository;
        private FlowSnapShot<Dto1> _resultA;
        private FlowSnapShot<Dto2> _resultB;
        private FlowSnapShot<Address> _resultC;

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
            _repository = new FlowSnapshotRepo<FlowSnapshot>(Environment.CurrentDirectory);
        }

        protected void AndGivenAWiredUpContainer()
        {
            _cb = new ContainerBuilder();

            _cb.RegisterGeneric(typeof(SpecProvider<>))
                .As(typeof(IProvideSpecifications<>))
                .As(typeof(SpecProvider<>))
                .SingleInstance();

            _cb.RegisterGeneric(typeof(Processor<,>))
                .As(typeof(Processor<,>));

            _cb.RegisterGeneric(typeof(ProcessorEngine<>))
                .As(typeof(ProcessorEngine<>));

            _cb.RegisterGeneric(typeof(ProcessorConfiguration<,>))
                .As(typeof(ProcessorConfiguration<,>));

            //specialist
            _cb.Register(m =>
            {
                var s = new SpecProvider<Dto1>();

                s.Add(a =>
                {
                    if (a.Source.Key == "Key-5")
                        return new SpecResult("Arbitrary invalid reason.");

                    return SpecResult.Success;
                }, "Key", "Description");

                s.MergeFrom(mq => mq.Contact, m.Resolve<SpecProvider<Contact>>());
                return s;
            }).AsSelf().AsImplementedInterfaces();

            _cb.Register(m =>
            {
                var s = new SpecProvider<Address>();

                s.Add(a =>
                {
                    if (a.ContactId == 0)
                        return new SpecResult("Contact Id has not been assigned.");

                    return SpecResult.Success;
                }, "Key", "Description");
                return s;
            }).AsSelf().AsImplementedInterfaces();

            _cb.Register(m =>
            {
                var s = new FlowSnapshotRepo<FlowSnapshot>(Environment.CurrentDirectory);
                return s;
            }).AsSelf().AsImplementedInterfaces();

            _container = _cb.Build();
        }

        protected void AndGivenAbatchProcess()
        {
            _batchProcess = new FlowBatch(_flow, BatchId);
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
                        var entity = new DenormalizedEntity
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
            var processorA = _container.Resolve<Processor<DenormalizedEntity, Dto1>>();

            processorA.BeginProcessingItem = (dtoIn, dtoOut) =>
            {
                dtoOut.Source = dtoIn;
                dtoOut.Contact = new Contact
                {
                    Id = 0,
                    Name = dtoIn.ContactName,
                    Description = dtoIn.ContactDescription
                };
            };

            _processorA = processorA;

            // process from csv
            var enumeration = new CsvProcessorReader<DenormalizedEntity>().Read(DataFileToCreate);
            _resultA = processorA.Process(enumeration, _batchProcess);

            _repository.Save(_resultA);
        }

        protected void AndWhenChainingTheProcessedResultsToAnotherProcessor()
        {
            // chain 2
            var processorB = _container.Resolve<Processor<Dto1, Dto2>>();

            processorB.BeginProcessingItem = (dtoIn, dtoOut) =>
            {
                // map object and preserve incoming result
                dtoOut.Source = dtoIn.Source;
                dtoOut.Contact = dtoIn.Contact;
                // don't update FK references or database ids
                dtoOut.Addresses = new[]
                {
                    new Address
                    {
                        StreetName = dtoIn.Source.Address1
                    },
                    new Address
                    {
                        StreetName = dtoIn.Source.Address2
                    }
                };
            };
            // save to database
            processorB.OnCommitting = processedItems =>
            {
                // save contacts to database
                using (var db = new TestModel())
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    db.Contacts.AddRange(processedItems.Select(m => m.Contact));

                    // save results to update contact idss
                    db.SaveChanges();

                    // update mappings and fk references
                    // ReSharper disable once PossibleMultipleEnumeration
                    processedItems.Each(m => { m.Addresses.Each(n => { n.ContactId = m.Contact.Id; }); });
                }
            };

            _resultB = processorB.Process(_resultA.Valid, _batchProcess);

            _repository.Save(_resultB);
        }

        protected void AndWhenChainingTheProcessedResultsToLastProcessor()
        {
            // chain 2
            var processorC = _container.Resolve<Processor<Dto2, Address>>();

            processorC.CreateOutput = dtoIn => dtoIn.Addresses;
            // save to database
            processorC.OnCommitting = processedItems =>
            {
                // save addresses now to database and update fk reference obtained above
                using (var db = new TestModel())
                {
                    db.Addresses.AddRange(processedItems);

                    // save results
                    db.SaveChanges();
                }
            };

            _resultC = processorC.Process(_resultB.Valid, _batchProcess);

            _repository.Save(_resultC);
        }

        protected void ThenResultsShouldBeValid()
        {
            Assert.NotNull(_resultA);
            Assert.NotNull(_resultB);
            Assert.NotNull(_resultB.Valid.First().Addresses.First().StreetName);
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
            var repo =
                new FlowSnapshotRepo<FlowSnapShot<Dto1>>(Environment.CurrentDirectory);

            var targetEntityType = new FlowEntity(typeof(Dto2));

            var result = repo.Get(targetEntityType.EntityTypeId, _flow.Code, BatchId);

            Assert.NotNull(result);
            Assert.Equal(CsvItemsToCreate - 1, result.ProcessedCount);
            Assert.Empty(result.Errors);
        }

        [BddfyFact]
        public void CanProcessIncomingDataInFlowsToFileAndSqlServerWithAutofac()
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