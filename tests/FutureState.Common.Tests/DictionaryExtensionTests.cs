using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FutureState.Common.Tests
{
    public class DictionaryExtensionTests
    {
        [Fact]
        public void CreateUniqueDictionaryFromList()
        {
            var list = new List<TestClass>()
            {
                new TestClass() {Id = 1, Name = "Name"},
                new TestClass() {Id = 1, Name = "Name 2"}
            };

            var dict = list.ToUniqueDictionary(m => m.Id, m => m.Name);

            Assert.Single(dict);

            Assert.Equal("Name", dict.Values.First());
        }


        [Fact]
        public void CanMergeDictionaries()
        {
            var dict = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 2"] = 2
            };

            var dict2 = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 3"] = 3
            };

            IDictionary<string, int> merged = dict.Merge(dict2);

            Assert.Equal(3, merged.Count);
            Assert.True(merged.ContainsKey("Name 3"));
        }

        [Fact]
        public void ComparesDictionaries()
        {
            var dict = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 2"] = 2
            };

            var dict2 = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 3"] = 3
            };

            Assert.False(dict.IsEquivalentTo(dict2));
        }


        [Fact]
        public void ComparesDictionariesFalse()
        {
            var dict = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 2"] = 2
            };

            var dict2 = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 2"] = 2
            };

            Assert.True(dict.IsEquivalentTo(dict2));
        }

        [Fact]
        public void GetGetValueOrDefault()
        {
            var dict = new Dictionary<string, int>
            {
                ["Name"] = 1,
                ["Name 2"] = 2
            };

            Assert.Equal(2, dict.Get("Name 2"));
            Assert.Equal(0, dict.Get("Name 3"));
        }


        public class TestClass
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
