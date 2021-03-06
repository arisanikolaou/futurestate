﻿using FutureState.Reflection;
using System;
using Xunit;

namespace FutureState.Common.Tests
{
    public class DefaultMapperTests
    {
        public class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TestEntity2 : TestEntity
        {
            public DateTime DateTime { get; set; }
        }

        [Fact]
        public void CanCopyOneEntityToAnother()
        {
            var source = new TestEntity
            {
                Id = 1,
                Name = "Name"
            };

            var copy = new TestEntity2();

            source.MapTo(copy);

            Assert.Equal(source.Name, copy.Name);
            Assert.Equal(source.Id, copy.Id);
        }
    }
}