
using FutureState.Flow.Data;
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
    public class CanGetSaveFlowSnapShotsStory
    {
        private FlowService _flowService;
        private FlowBatch _batch;

        protected void GivenANewFlowAndFlowBatch()
        {
            // clean up existing files
            foreach (var file in Directory.GetFiles(Environment.CurrentDirectory, "*SampleFlow*.json"))
                File.Delete(file);

            _flowService = new FlowService(new FlowIdRepo());
            _batch = _flowService.GetNewFlowBatch("SampleFlow");
        }

        protected void WhenSavingASnapShot()
        {
            var flowSnapShot = new FlowSnapShot<TargetType>(_batch);
            flowSnapShot.Valid = new List<TargetType>()
            {
                new TargetType()
                {
                    Name = "Name", MaybeDate = DateTime.UtcNow.Date, MaybeInt = 2
                },
                new TargetType()
                {
                    Name = "Name-2", MaybeDate = DateTime.UtcNow.Date, MaybeInt = 3
                }
            };
            var resultRepo = new FlowSnapshotRepo<FlowSnapShot<TargetType>>();

            resultRepo.Save(flowSnapShot);
        }

        protected void ThenShouldBeAbleToLoadTheSnapShot()
        {
            var flowSnapShot = new FlowSnapShot<TargetType>(_batch);
            var resultRepo = new FlowSnapshotRepo<FlowSnapShot<TargetType>>();

            var loadedSnapShot = resultRepo.Get(
                new FlowEntity(typeof(TargetType)).EntityTypeId, 
                "SampleFlow", 
                1);

            Assert.NotNull(loadedSnapShot);
            
            Assert.NotNull(loadedSnapShot.Valid.FirstOrDefault(m => m.Name == "Name"));
            Assert.NotNull(loadedSnapShot.Valid.FirstOrDefault(m => m.MaybeInt == 2));
            Assert.True(loadedSnapShot.Valid.Last()?.MaybeInt == 3);

            Assert.NotNull(loadedSnapShot.TargetType);
        }

        protected void ThenShouldNotBeAbleToLoadNonExistingSnapShot()
        {
            var flowSnapShot = new FlowSnapShot<TargetType>(_batch);
            var resultRepo = new FlowSnapshotRepo<FlowSnapShot<TargetType>>();

            var loadedSnapShot = resultRepo.Get(
                new FlowEntity(typeof(TargetType)).EntityTypeId,
                "SampleFlow",
                2); // batch does not exist

            Assert.Null(loadedSnapShot);
        }

        [BddfyFact]
        protected void CanGetSaveFlowSnapShots()
        {
            this.BDDfy();
        }

        public class TargetType
        {
            public string Name { get; set; }

            public DateTime MaybeDate { get; set; }


            public int MaybeInt { get; set; }
        }
    }
}
