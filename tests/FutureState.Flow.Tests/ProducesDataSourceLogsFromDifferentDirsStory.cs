using System;
using System.Diagnostics;
using System.IO;
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
        private DirectoryBaseDataSourceProducer _producer;
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
            _repo.Save(new DataSourceLog(_flowEntity, "*.csv"));
        }

        protected void WhenADataSourceProducerIsStartedAndValid()
        {
            var dir1 = new DirectoryInfo(_sourceDir1);
            var dir2 = new DirectoryInfo(_sourceDir2);

            var dirs = new[] { dir1, dir2 };

            var producer = new DirectoryBaseDataSourceProducer(dirs, _flowEntity, _repo)
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
            SpinWait(() => _repo.Get(_flowEntity).Entries.Count < 5);

            // ensure log populated with three entities.
            var log = _repo.Get(_flowEntity);

            Assert.Equal(5, log.Entries.Count);

            Assert.Contains(log.Entries, m => m.AddressId.Contains("File1.csv"));
            Assert.Contains(log.Entries, m => m.AddressId.Contains("File2.csv"));
            Assert.DoesNotContain(log.Entries, m => m.AddressId.Contains("File3.txt"));
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
            SpinWait(() => _repo.Get(_flowEntity).Entries.Count < 6);

            // ensure log populated with three entities.
            var log = _repo.Get(_flowEntity);

            Assert.Equal(6, log.Entries.Count);
        }

        protected void AndThenShouldBeAbleToStopTheProducer()
        {
            _producer.Stop();
        }

        protected void AndThenLogShouldBeAvailableAfterStopping()
        {
            // ensure log populated with three entities.
            var log = _repo.Get(_flowEntity);

            Assert.Equal(6, log.Entries.Count);
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
