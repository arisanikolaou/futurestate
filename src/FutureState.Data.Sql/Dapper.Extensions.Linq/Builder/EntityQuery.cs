using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Builder;
using Dapper.Extensions.Linq.Core.Implementor;
using Dapper.Extensions.Linq.Core.Predicates;
using Dapper.Extensions.Linq.Core.Sessions;
using Dapper.Extensions.Linq.Predicates;

namespace Dapper.Extensions.Linq.Builder
{
    public sealed class EntityQuery<T> : IEntityBuilder<T> where T : class
    {
        private readonly Expression<Func<T, bool>> _expression;
        private readonly IDapperSession _session;
        private readonly IList<ISort> _sort;
        private bool _nolock;
        private int? _take;
        private int? _timeout;
        readonly IDapperImplementor _implementor;

        public EntityQuery(IDapperImplementor implementor, IDapperSession session, Expression<Func<T, bool>> expression)
        {
            _session = session;
            _expression = expression;
            _implementor = implementor;
            _sort = new List<ISort>();
        }

        public IEnumerable<T> AsEnumerable()
        {
            return ResolveEnities();
        }

        public bool Any()
        {
            return ResolveEnities().Any();
        }

        public IList<T> ToList()
        {
            return ResolveEnities().ToList();
        }

        public int Count()
        {
            return ResolveEnities().Count();
        }

        public T Single()
        {
            return ResolveEnities().Single();
        }

        public T SingleOrDefault()
        {
            return ResolveEnities().SingleOrDefault();
        }

        public T FirstOrDefault()
        {
            return ResolveEnities().FirstOrDefault();
        }

        public IEntityBuilder<T> OrderBy(Expression<Func<T, object>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            if (propertyInfo == null) return this;

            var sort = new Sort
            {
                PropertyName = propertyInfo.Name,
                Ascending = true
            };
            _sort.Add(sort);

            return this;
        }

        public IEntityBuilder<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            var propertyInfo = ReflectionHelper.GetProperty(expression) as PropertyInfo;
            if (propertyInfo == null) return this;

            var sort = new Sort
            {
                PropertyName = propertyInfo.Name,
                Ascending = false
            };
            _sort.Add(sort);

            return this;
        }

        public IEntityBuilder<T> Take(int number)
        {
            _take = number;
            return this;
        }

        /// <summary>
        ///     Timeouts cannot be specified for SqlCe, these will remain zero.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public IEntityBuilder<T> Timeout(int timeout)
        {
            _timeout = timeout;
            return this;
        }

        public IEntityBuilder<T> Nolock()
        {
            _nolock = true;
            return this;
        }

        private IEnumerable<T> ResolveEnities()
        {
            var predicate = QueryBuilder<T>.FromExpression(_expression);

            var p = predicate?.Predicates == null ? null : predicate;

            return _implementor.GetList<T>(_session.Connection, p, _sort, _session.Transaction, _timeout, false, _take, _nolock);
        }
    }
}