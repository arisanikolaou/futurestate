﻿using Dapper.Extensions.Linq.Core.Predicates;
using System.Collections.Generic;

namespace Dapper.Extensions.Linq.Predicates
{
    public class SequenceReaderResultReader : IMultipleResultReader
    {
        private readonly Queue<SqlMapper.GridReader> _items;

        public SequenceReaderResultReader(IEnumerable<SqlMapper.GridReader> items)
        {
            _items = new Queue<SqlMapper.GridReader>(items);
        }

        public IEnumerable<T> Read<T>()
        {
            var reader = _items.Dequeue();
            return reader.Read<T>();
        }
    }
}