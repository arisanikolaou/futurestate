using FutureState.Flow.Data;
using FutureState.Flow.Model;
using System;
using System.IO;
using System.Linq;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    public class FlowFilesCanBeTreatedAsDataFileSourcesStory
    {
        const string _dataSourceDir = "FlowFilesAreDataSources";
        private FlowFileLogRepo _flowFileRepo;
        private DataSourceLogRepo _dataFileRepo;
        private FlowFileLog _flowFileDb;
        private DataFileLog _dataSourceDb;

        protected void GivenAFlowFileDataSourceAndRepository()
        {
            if (Directory.Exists(_dataSourceDir))
                Directory.Delete(_dataSourceDir, true);

            Directory.CreateDirectory(_dataSourceDir);

            var repo = new FlowFileLogRepo(_dataSourceDir);

            this._flowFileRepo = repo;
        }

        protected void AndGivenANumberOfFlowFilesHaveBeenProduced()
        {
            var lastUpdateDate = DateTime.UtcNow;

            _flowFileRepo.Add(typeof(TestEntity1), "code", new FlowFileLogEntry()
            {
                AddressId = "Path1.txt",
                TargetAddressId = "Target1",
                DateLastUpdated = lastUpdateDate
            });

            _flowFileRepo.Add(typeof(TestEntity1), "code", new FlowFileLogEntry()
            {
                AddressId = "Path2.txt",
                TargetAddressId = "Target2",
                DateLastUpdated = lastUpdateDate
            });

            _flowFileRepo.Add(typeof(TestEntity1), "code", new FlowFileLogEntry()
            {
                AddressId = "Path2.txt",
                TargetAddressId = "Target2",
                DateLastUpdated = DateTime.UtcNow
            });
        }

        protected void WhenReadingTheFlowFileAsASimpleDataSourceFile()
        {
            // flow files and data files are the same
            _dataFileRepo = new DataSourceLogRepo(_dataSourceDir);

            this._flowFileDb = _flowFileRepo.Get(typeof(TestEntity1), "code");
            this._dataSourceDb = _dataFileRepo.Get(typeof(TestEntity1));
        }

        protected void ThenTheConsumableDataSourceFlowFilesShouldBeQueryable()
        {
            Assert.NotNull(_flowFileDb);
            Assert.Equal(3, _flowFileDb.Entries.Count);
            // contains
            Assert.NotNull(_flowFileRepo.Get(typeof(TestEntity1), "Path1.txt"));

            
            Assert.NotNull(_dataSourceDb);
            Assert.Equal(3, _dataSourceDb.Entries.Count);
            Assert.True(_dataFileRepo.Contains(typeof(TestEntity1), "Path1.txt"));
        }

        [BddfyFact]
        public void FlowFilesCanBeTreatedAsDataFileSources()
        {
            this.BDDfy();
        }

        public class TestEntity1
        {
            public string Name { get; set; }
        }
    }
}
