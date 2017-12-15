#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    /// Immutable structure used to communicate how many entities were added, updated and/or deleted in a batch.
    /// </summary>
    public class BatchUpdateResult
    {
        public BatchUpdateResult(int added = 0, int updated = 0, int deleted = 0)
        {
            Guard.Ensure(added >= 0, () => new ArgumentOutOfRangeException(nameof(added)));
            Guard.Ensure(updated >= 0, () => new ArgumentOutOfRangeException(nameof(updated)));
            Guard.Ensure(deleted >= 0, () => new ArgumentOutOfRangeException(nameof(deleted)));

            Added = added;
            Updated = updated;
            Deleted = deleted;
        }

        /// <summary>
        /// The entities that were added during the last update routine.
        /// </summary>
        public int Added { get; }

        /// <summary>
        /// The entities that were deleted during the last update routine.
        /// </summary>
        public int Deleted { get; }

        /// <summary>
        /// An empty batch result.
        /// </summary>
        public static BatchUpdateResult Empty { get; } = new BatchUpdateResult();

        /// <summary>
        /// The entities that were updated during the last update routine.
        /// </summary>
        public int Updated { get; }

        /// <summary>
        /// Combines one batch result with another and rolls it up.
        /// </summary>
        public static BatchUpdateResult Combine(params BatchUpdateResult[] batchUpdateResults)
        {
            int added = 0, updated = 0, deleted = 0;

            batchUpdateResults.Each(m =>
                                    {
                                        added += m.Added;
                                        updated += m.Updated;
                                        deleted += m.Deleted;
                                    });

            return new BatchUpdateResult(added, updated, deleted);
        }

        /// <summary>
        /// Combines one batch result with another and rolls it up.
        /// </summary>
        public BatchUpdateResult Add(BatchUpdateResult other)
        {
            return Combine(this, other);
        }
    }
}