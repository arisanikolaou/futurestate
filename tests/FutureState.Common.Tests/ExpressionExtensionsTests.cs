using System;
using System.Linq.Expressions;
using Xunit;

namespace FutureState.Common.Tests
{
    public class ExpressionExtensionsTests
    {
        public class TestEntity
        {
            public int Id { get; set; }

            public void Method1()
            {
            }
        }

        [Fact]
        public void GetsNameFromPropertyExpression()
        {
            Expression<Func<TestEntity, object>> expression = entity => entity.Id;

            var name = expression.GetPropertyName();

            Assert.Equal("Id", name);
        }

        [Fact]
        public void GetsPropertyInfoFromExpression()
        {
            Expression<Func<TestEntity, int>> expression = entity => entity.Id;

            var propertyInfo = expression.GetMemberInfo();

            Assert.NotNull(propertyInfo);
            Assert.Equal("Id", propertyInfo.Name);
        }
    }
}