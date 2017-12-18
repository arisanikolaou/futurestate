#region

using System.Collections.Generic;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     The response of a paging request against a page-able repository.
    /// </summary>
    /// <typeparam name="TEntities">The entities to be displayed in a page.</typeparam>
    public sealed class PageResponse<TEntities>
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="items">The entities in the given page.</param>
        /// <param name="totalCount">The total entities returned in the query.</param>
        public PageResponse(IEnumerable<TEntities> items, long totalCount)
        {
            Guard.ArgumentNotNull(items, nameof(items));

            TotalCount = totalCount;
            Items = items;
        }

        /// <summary>
        ///     Gets the number of items in the page.
        /// </summary>
        public IEnumerable<TEntities> Items { get; }

        /// <summary>
        ///     Gets the total number of entities available to a given query.
        /// </summary>
        public long TotalCount { get; }
    }
}