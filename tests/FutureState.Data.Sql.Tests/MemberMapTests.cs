using FutureState.Data.Sql.Mappings;
using System.Reflection;
using Xunit;

namespace FutureState.Data.Sql.Tests
{
    public class MemberMapTests
    {
        [Fact()]
        public void MemberMapMapsEntityFieldsAndProperties()
        {
            foreach (var member in typeof(TestEntity).GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                var memberMap = new MemberMap(member, member.Name);

                Assert.Equal(member.Name, memberMap.ColumnName);

                Assert.Equal(typeof(string), memberMap.MemberType);
            }
        }

        public class TestEntity
        {
            public string Field { get; set; }
            public string Name { get; set; }

            private int PrivateMethod()
            {
                return 0;
            }
        }
    }
}
