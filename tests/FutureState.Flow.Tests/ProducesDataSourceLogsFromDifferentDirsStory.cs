using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    public class ProducesDataSourceLogsFromDifferentDirsStory
    {
        private string _sourceDir1;
        private string _logDir;
        private DataSourceLogRepo _repo;
        private FlowEntity _flowEntity;
        private DirectoryBasedDataSourceProducer _producer;
        private string _sourceDir2;

        protected void GivenASetOfCsvFilesInAGivenSouceDir()
        {
            _sourceDir1 = "FlowDataSource1";

            // ensure clean
            if (Directory.Exists(_sourceDir1))
                Directory.Delete(_sourceDir1, true);

            Directory.CreateDirectory(_sourceDir1);

            using (var file = File.CreateText($@"{_sourceDir1}\File1.csv"))
                file.Flush();

            using (var file = File.CreateText($@"{_sourceDir1}\File2.csv"))
                file.Flush();
        }

        protected void GivenASetOfCsvFilesInAnotherSouceDir()
        {
            _sourceDir2 = "FlowDataSource2";

            // ensure clean
            if (Directory.Exists(_sourceDir2))
                Directory.Delete(_sourceDir2, true);

            Directory.CreateDirectory(_sourceDir2);
            
            // should be recognized as unique
            using (var file = File.CreateText($@"{_sourceDir2}\File1.csv"))
                file.Flush();

            // should be recognized as unique
            using (var file = File.CreateText($@"{_sourceDir2}\File2.csv"))
                file.Flush();
        }

        protected void AndGivenADataSourceLogDir()
        {
            _logDir = "FlowProducerLog";

            // ensure clean
            if (Directory.Exists(_logDir))
                Directory.Delete(_logDir, true);

            Directory.CreateDirectory(_logDir);
        }

        protected void AndGivenADataSourceLogExistsAndIsValidAndIncludingSpecificFileTypes()
        {
            // flow entity 
            _flowEntity = FlowEntity.Create<TestEntity>();

            _repo = new DataSourceLogRepo(_logDir);
            _repo.Save(new DataFileLog(_flowEntity, "*.csv"));
        }

        protected void WhenADataSourceProducerIsStartedAndValid()
        {
            var dir1 = new DirectoryInfo(_sourceDir1);
            var dir2 = new DirectoryInfo(_sourceDir2);

            var dirs = new[] { dir1, dir2 };

            var producer = new DirectoryBasedDataSourceProducer(dirs, _flowEntity, _repo)
            {
                PollInterval = TimeSpan.FromMilliseconds(500)
            };

            // start timer
            producer.Start();

            _producer = producer;
        }

        protected void AndWhenWritingMoreFilesToAGivenDirectory()
        {
            using (var file = File.CreateText($@"{_sourceDir1}\File3.csv"))
                file.Close();

            using (var file = File.CreateText($@"{_sourceDir1}\File3.txt"))
                file.Close();
        }

        protected void ThenTheFilesShouldBePickedUpIntoTheLog()
        {
            SpinWait(() => _repo.GetEntries(_flowEntity).Count() < 5);

            // ensure log populated with three entities.
            var log = _repo.Get(_flowEntity);

            Assert.Equal(5, log.Entries.Count);

            Assert.True(_repo.Contains(_flowEntity, Path.GetFullPath($@"{_sourceDir1}\File1.csv")));
            Assert.True(_repo.Contains(_flowEntity, Path.GetFullPath($@"{_sourceDir1}\File2.csv")));

            Assert.False(_repo.Contains(_flowEntity, Path.GetFullPath($@"{_sourceDir1}\File3.txt")));
        }

        protected void AndThenUpdatingAnExistingDataSourceFile()
        {
            string filePath = $@"{_sourceDir1}\File3.csv";

            File.Delete(filePath);

            using (var file = File.CreateText(filePath))
                file.Close();
        }


        protected void AndThenTheUpdatedFileShouldBeAddedToTheLog()
        {
            SpinWait(() => _repo.Count(_flowEntity) < 6);

            // ensure log populated with three entities.
            var log = _repo.Get(_flowEntity);

            Assert.Equal(6, _repo.Count(_flowEntity));
        }

        protected void AndThenShouldBeAbleToStopTheProducer()
        {
            _producer.Stop();
        }

        protected void AndThenLogShouldBeAvailableAfterStopping()
        {
            // ensure log populated with three entities.
            var entriesCount = _repo.Count(_flowEntity);

            Assert.Equal(6, entriesCount);
        }

        [BddfyFact]
        public void ProducesDataSourceLogsFromDifferentDirs()
        {
            this.BDDfy();
        }

        public class TestEntity
        {

        }


        void SpinWait(Func<bool> condition, int maxSeconds = 5)
        {
            var sw = new Stopwatch();
            sw.Start();
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                // spin wait 5 seconds
                if (sw.Elapsed.TotalSeconds >= maxSeconds)
                    break;
            }
            while (condition());

            sw.Stop();
        }
    }
}
