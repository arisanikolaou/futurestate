using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using FutureState.Flow.Data;
using FutureState.Specifications;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace FutureState.Flow.Enrich
{
    [Story()]
    public class EnricherControllerCanEnrichFromCsvStory
    {
        private FlowId _flow;
        private FlowBatch _flowBatch;
        private string _workingDirectory;
        private FlowSnapShot<Whole> _processResult;
        private FlowSnapshotRepo<FlowSnapShot<Whole>> _repo;
        private string _wholeDir;
        private string _dataFileToCreate;
        private int CsvItemsToCreate = 10;
        private EnricherController<Whole> _subject;
        private string _partDir;

        protected void GivenAFlowAndABatchProcess()
        {
            this._flow = new FlowId("Test");
            this._flowBatch = new FlowBatch(_flow, 1);
        }

        protected void AndGivenACleanWorkingDirectory()
        {
            this._workingDirectory = $@"{Environment.CurrentDirectory}\Enrichers";

            if (Directory.Exists(_workingDirectory))
                Directory.Delete(_workingDirectory, true);

            Directory.CreateDirectory(_workingDirectory);
        }

        protected void AndGivenAProcessResult()
        {
            this._processResult = new FlowSnapShot<Whole>()
            {
                SourceType = new FlowEntity(typeof(Part)),
                TargetType = new FlowEntity(typeof(Whole)),
                Batch = _flowBatch,
                Invalid = new List<Whole>(),
                Valid = new List<Whole>() // valid entities
                {
                    new Whole(){Key = "Key1", FirstName = "A", LastName = "LastName"},
                    new Whole(){Key = "Key2", FirstName = "", LastName = ""},
                }
            };

            _wholeDir = $@"{_workingDirectory}\Whole";

            _repo = new FlowSnapshotRepo<FlowSnapShot<Whole>>(_wholeDir);
            _repo.Save(_processResult);
        }

        protected void AndGivenACsvFileToEnrich()
        {
            this._partDir = $@"{_workingDirectory}\Part";
            Directory.CreateDirectory(_partDir);


            _dataFileToCreate = $@"{_partDir}\DataFile.csv";
            if (File.Exists(_dataFileToCreate))
                File.Delete(_dataFileToCreate);

            // create mock csv
            using (var fs = File.OpenWrite(_dataFileToCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    var csv = new CsvWriter(sw);
                    csv.Configuration.HasHeaderRecord = true; //this should be the default value

                    csv.WriteHeader<Part>();

                    csv.Flush();
                    csv.NextRecord();


                    for (var i = 0; i < CsvItemsToCreate; i++)
                    {
                        var entity = new Part()
                        {
                            Key = $"Key{i}",
                            FirstName = $"FirstName{i}"
                        };

                        csv.WriteRecord(entity);
                        csv.NextRecord();
                    }

                    csv.Flush();
                }
            }
        }

        protected void AndGivenAnEnrichingController()
        {
            _subject = new EnricherController<Whole>(
                new ProcessorConfiguration<Whole, Whole>(new SpecProvider<Whole>(),
                    new SpecProvider<IEnumerable<Whole>>()), _workingDirectory);
        }

        protected void WhenProcessingTheEnrichment()
        {
            // get list of enrichers
            var enrichers = new List<Enricher<Part, Whole>>();
            foreach (var file in Directory.GetFiles(_partDir))
            {
                var enricherBuilder = new CsvEnricherBuilder<Part, Whole>() { FilePath = file };
                enrichers.Add(enricherBuilder.Get());
            }

            // get list of files to process
            var targetEnrichers = new List<EnrichmentTarget<WholeSource, Whole>>();
            foreach (var file in new DirectoryInfo(_wholeDir).GetFiles())
            {
                var repository = new FlowSnapshotRepo<FlowSnapShot<Whole>>();

                targetEnrichers.Add(new EnrichmentTarget<WholeSource, Whole>(repository)
                {
                    DataSource = file
                });
            }

            // process results
            foreach (var target in targetEnrichers)
                _subject.Process(_flowBatch, enrichers, target);
        }


        protected void ThenANewProcessOutputFileShouldBeUsed()
        {
            //todo
        }


        [BddfyFact]
        public void EnricherControllerTests()
        {
            this.BDDfy();
        }
    }
}
