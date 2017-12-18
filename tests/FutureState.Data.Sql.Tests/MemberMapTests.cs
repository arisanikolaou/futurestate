using System.Reflection;
using FutureState.Data.Sql.Mappings;
using Xunit;

namespace FutureState.Data.Sql.Tests
{
    public class MemberMapTests
    {
        public class TestEntity
        {
            public string Field { get; set; }
            public string Name { get; set; }

            private int PrivateMethod()
            {
                return 0;
            }
        }

        [Fact]
        public void MemberMapMapsEntityFieldsAndProperties()
        {
            foreach (var member in typeof(TestEntity).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                var memberMap = new MemberMap(member, member.Name);

                Assert.Equal(member.Name, memberMap.ColumnName);

                Assert.Equal(typeof(string), memberMap.MemberType);
            }
        }
    }
}