using System;
using System.IO;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Flow.Tests
{
    [Story]
    public class CanCreateUniqueFlowsStory
    {
        private FlowIdRepo _repo;
        private FlowService _flowService;
        private FlowId _flow1;
        string _flowCode = "FlowCode";

        protected void GivenANewFlowService()
        {
            _repo = new FlowIdRepo();
            _flowService = new FlowService(_repo);
        }

        protected void WhenCreatingANewFlow()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, $"*{_flowCode}*.json");

            foreach (var file in files)
                File.Delete(file);

            _flow1 = _flowService.CreateNew(_flowCode);
        }

        protected void ThenFlowShouldBeCreated()
        {
            Assert.NotNull(_flow1);
        }

        protected void ThenWhenCreatingAnotherServiceWithTheSameNameShouldFail()
        {
            Assert.Throws<InvalidOperationException>(() => _flowService.CreateNew(_flowCode));
        }

        protected void ThenShouldBeAbleToCreateNewBatchesInSequenceFromFlow()
        {
            var batch1 = _flowService.GetNewFlowBatch(_flow1.Code);
            var batch2 = _flowService.GetNewFlowBatch(_flow1.Code);

            Assert.Equal(batch1.BatchId + 1, batch2.BatchId);
        }

        [BddfyFact]
        public void CanCreateUniqueFlows()
        {
            this.BDDfy();
        }
    }
}
