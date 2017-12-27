using System;

namespace FutureState.Data
{
    public interface IPagedReader<TEntity, in TKey> : IReader<TEntity, TKey>,
        IGetter<PageResponse<TEntity>, Action<IPageRequest<TEntity>>>
    {
    }

    public interface IPagedReader<TEntity> : IPagedReader<TEntity, Guid>, IReader<TEntity>
    {
    }
}