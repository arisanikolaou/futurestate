using System;
using System.Linq.Expressions;
using TestStack.BDDfy;
using TestStack.BDDfy.Xunit;
using Xunit;

namespace FutureState.Common.Tests
{
    [Story]
    public class CombinesAndOrExpressionsStory
    {
        private Expression<Func<TestEntity, bool>> _expression;
        private Expression<Func<TestEntity, bool>> _expressionOr;
        private Expression<Func<TestEntity, bool>> _expressionAnd;

        protected void GivenAnExpression()
        {
            _expression = e => e.Name == "Name";
        }

        protected void WhenOrIngAnExpression()
        {
            _expressionOr = _expression.Or(m => m.Name == "Name 2");
        }

        protected void WhenAndingIngAnExpression()
        {
            _expressionAnd = _expression.And(m => m.Id == 1);
        }

        protected void ThenOrExpressionShouldBeValid()
        {

            var ex = _expressionOr.Compile();

            Assert.True(ex.Invoke(new TestEntity() { Name = "Name" }));
            Assert.False(ex.Invoke(new TestEntity() { Name = "Name 3" }));
        }

        protected void ThenAndExpressionShouldBeValid()
        {
            var ex = _expressionAnd.Compile();

            Assert.True(ex.Invoke(new TestEntity() { Name = "Name", Id = 1 }));
            Assert.False(ex.Invoke(new TestEntity() { Name = "Name 3", Id = 2 }));
        }

        [BddfyFact]
        public void CombinesAndOrExpressions()
        {
            this.BDDfy();
        }

        public class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
