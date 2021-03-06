﻿using System;
using System.Collections.Generic;
using Xunit;

namespace FutureState.Common.Tests
{
    public class SeqGuidTests
    {
        [Fact]
        public void GeneratesUniqueSeqGuids()
        {
            // arrange
            var list = new List<Guid>();
            for (var i = 0; i < 10; i++)
                list.Add(SeqGuid.Create());

            // assert
            for (var i = 0; i < list.Count; i++)
            for (var j = i + 1; j < list.Count; j++)
                if (list[i] == list[j])
                    throw new Exception("Generated guids are not unique.");
        }
    }
}