using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using FutureState.Flow.Data;
using FutureState.Flow.Tests.Flow;
using FutureState.Specifications;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;

namespace FutureState.Flow.Tests.Aggregators
{
    [Story()]
    public class EnricherControllerCanEnrichFromCsvStories
    {
        private BatchProcess _batchProcess;
        private string _workingDirectory;
        private ProcessResult<WholeSource, Whole> _processResult;
        private ProcessResultRepository<ProcessResult<WholeSource, Whole>> _repo;
        private string _wholeDir;
        private string _dataFileToCreate;
        private int CsvItemsToCreate = 10;
        private EnricherController2 _subject;
        private string _partDir;

        protected void GivenAFlowAndABatchProcess()
        {
            this._batchProcess = new BatchProcess()
            {
                FlowId = Guid.Parse("f41cfe3a-4ddb-43ae-8302-0c322c84bdd2"),
                BatchId = 1
            };
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
            this._processResult = new ProcessResult<WholeSource, Whole>()
            {
                ProcessName = "TestProcess",
                BatchProcess = _batchProcess,
                Input = new List<WholeSource>(),
                Invalid = new List<Whole>(),
                Output = new List<Whole>()
                {
                    new Whole(){Key = "Key1", FirstName = "A", LastName = "LastName"},
                    new Whole(){Key = "Key2", FirstName = "", LastName = ""},
                }
            };

            _wholeDir = $@"{_workingDirectory}\Whole";

            _repo = new ProcessResultRepository<ProcessResult<WholeSource, Whole>>(_wholeDir);
            _repo.Save(_processResult);
        }

        protected void AndGivenACsvFileToEnrich()
        {
            this._partDir = $@"{_workingDirectory}\Part";
            Directory.CreateDirectory(_partDir);


            _dataFileToCreate = $@"{_partDir}\DataFile.csv";
            if (File.Exists(_dataFileToCreate))
                File.Delete(_dataFileToCreate);

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
            _subject = new EnricherController2(
                new ProcessorConfiguration<Whole, Whole>(new SpecProvider<Whole>(),
                    new SpecProvider<IEnumerable<Whole>>()), _workingDirectory);
        }

        protected void WhenProcessingTheEnrichment()
        {
            var enrichers = new List<Enricher<Part, Whole>>();
            
            foreach (var file in Directory.GetFiles(_partDir))
            {
                var enricherBuilder = new CsvEnricherBuilder<Part, Whole>() {FileName = file};
                enrichers.Add(enricherBuilder.Get());
            }
            

            _subject.Process(_batchProcess.FlowId, enrichers.ToArray(), new DirectoryInfo(_wholeDir));
        }

        protected void ThenANewProcessOutputFileShouldBeUsed()
        {

        }


        [BddfyFact]
        public void EnricherControllerTests()
        {
            this.BDDfy();
        }
    }
}
