using System;
using System.Linq.Expressions;
using Xunit;

namespace FutureState.Common.Tests
{
    public class ExpressionExtensionsTests
    {
        [Fact]
        public void GetsPropertyInfoFromExpression()
        {
            Expression<Func<TestEntity, int>> expression = entity => entity.Id;

            var propertyInfo = expression.GetMemberInfo();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Id", propertyInfo.Name);
        }

        [Fact]
        public void GetsNameFromPropertyExpression()
        {
            Expression<Func<TestEntity, object>> expression = entity => entity.Id;

            var name = expression.GetPropertyName();

            Assert.Equal("Id", name);
        }

        public class TestEntity
        {
            public int Id { get; set; }

            public void Method1()
            {

            }
        }
    }
}
