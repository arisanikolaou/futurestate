using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EmitMapper;

namespace FutureState.Data.Sql
{
    public class RepositoryLinq<TEntity> : RepositoryLinq<TEntity, Guid>, IRepositoryLinq<TEntity>, IRepository<TEntity>
        where TEntity : class, IEntity<Guid>
    {
        public RepositoryLinq(ISession session) : base(session)
        {
        }
    }

    public class RepositoryLinq<TEntity, TKey> : Repository<TEntity, TKey>, IRepositoryLinq<TEntity, TKey>,
        IRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private readonly EntityQueryBuilder<TEntity> _queryBuilder;

        public RepositoryLinq(ISession session) : base(session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            _queryBuilder = new EntityQueryBuilder<TEntity>(session);
        }

        public PageResponse<TEntity> Get(Action<IPageRequest<TEntity>> key)
        {
            throw new NotImplementedException();
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryBuilder.GetQuery(predicate).Any();
        }

        public long Count(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryBuilder.GetQuery(predicate).Count();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryBuilder.GetQuery(predicate).FirstOrDefault();
        }

        public IEnumerable<TEntity> GetByKeys<TQueryArg>(
            IEnumerable<TQueryArg> queryArgs,
            Expression<Func<TEntity, TQueryArg, bool>> matchExpression)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TProjection> Select<TProjection>(Expression<Func<TEntity, bool>> predicate)
            where TProjection : new()
        {
            var queryBuilder = _queryBuilder.GetQuery(predicate);

            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TEntity, TProjection>();

            return queryBuilder
                .ToList()
                .Select(m => mapper.Map(m));
        }

        public IEnumerable<TProjection> Select<TProjection>() where TProjection : new()
        {
            var queryBuilder = _queryBuilder.GetQuery(m => true);

            // TODO: optimize to select projection only
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TEntity, TProjection>();

            return queryBuilder
                .ToList()
                .Select(m => mapper.Map(m));
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryBuilder.GetQuery(predicate).FirstOrDefault();
        }

        public IEnumerable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return _queryBuilder.GetQuery(predicate).ToList();
        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            // todo: optimize
            var results = _queryBuilder.GetQuery(predicate).ToList();

            results.Each(m => DeleteById(m.Id));
        }
    }
}