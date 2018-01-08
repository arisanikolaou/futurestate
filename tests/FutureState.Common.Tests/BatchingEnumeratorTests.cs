using System.Collections.Generic;
using FutureState.IO;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Common.Tests
{
    [Story]
    public class BatchingEnumeratorTests
    {
        List<TestEntity> _list;
        int _batches;
        int _totalItems;

        protected void GivenAEnumeration()
        {
            this._list = new List<TestEntity>();
            for (int i = 0; i < 100; i++)
                _list.Add(new TestEntity { Id = i });
        }

        protected void WhenRequestingBatchesOfNotMoreThanXUnits()
        {
            var enumerator = new EnumerableBatcher<TestEntity>(_list, 60);

            while (enumerator.MoveNext())
            {
                _batches++;

                foreach (var item in enumerator.GetCurrentItems())
                {
                    Assert.Equal(item.Id, _totalItems);

                    _totalItems++;
                }
            }
        }

        protected void ThenIShouldReceiveAllItemsInEnumerationsInTheRequestedBatchSizeWindows()
        {
            Assert.Equal(100, _totalItems);
            Assert.Equal(2, _batches);
        }

        [BddfyFact]
        public void CanSplitPathIntoLists()
        {
            this.BDDfy();
        }


        public class TestEntity
        {
            public int Id { get; set; }
        }
    }
}
