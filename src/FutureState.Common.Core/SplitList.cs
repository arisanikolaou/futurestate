using System;
using System.Collections.Generic;

namespace FutureState
{
    public class SplitList<TEntity>
        where TEntity : IEquatable<TEntity>
    {
        public List<TEntity> Existing { get; set; } = new List<TEntity>();
        public List<TEntity> New { get; set; } = new List<TEntity>();

        public void Process(
            IEnumerable<TEntity> existingList,
            IEnumerable<TEntity> sourceList,
            Action<TEntity, TEntity> handleExisting)
        {
            foreach (var source in sourceList)
            {
                var isNew = true;

                foreach (var existingItem in existingList)
                    if (source.Equals(existingItem))
                    {
                        handleExisting(source, existingItem);
                        Existing.Add(existingItem);
                        isNew = false;
                        break;
                    }

                if (isNew)
                    New.Add(source);
            }
        }
    }
}