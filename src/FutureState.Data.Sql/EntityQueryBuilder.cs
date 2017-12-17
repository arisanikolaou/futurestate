using System;
using System.Linq.Expressions;
using Dapper.Extensions.Linq.Builder;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Implementor;
using Dapper.Extensions.Linq.Sql;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     Helps construct a dapper linq query given a boxed ISession.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to query for.</typeparam>
    public class EntityQueryBuilder<TEntity> where TEntity : class
    {
        readonly DapperImplementor _dapperImplementation;
        readonly DapperSession _dapperSession;

        public EntityQueryBuilder(ISession session)
        {
            Guard.ArgumentNotNull(session, nameof(session));

            var sqlException = session as Session;
            if (sqlException != null)
            {
                var sqlGeneratorImpl = new SqlGeneratorImpl(sqlException.Configuration);

                _dapperImplementation = new DapperImplementor(sqlGeneratorImpl);
            }
            else
            {
                throw new InvalidOperationException("ISession is not a Session type.");
            }

            _dapperSession = new DapperSession(sqlException.GetConnection());
        }

        public EntityQuery<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate)
        {
            return new EntityQuery<TEntity>(_dapperImplementation, _dapperSession, predicate);
        }
    }
}