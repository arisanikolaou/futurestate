#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace FutureState
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Combines two expressions with AND logic (return true if expression 1 is true AND expression 2 is true).
        /// </summary>
        /// <typeparam name="T">The type of object being examined in both expressions.</typeparam>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression, will be combined with the first into a single expression.</param>
        /// <returns>An expression which combines the two given expressions with an AND operation.</returns>
        /// <remarks>This is a handy way to combine expression clauses for Linq-to-SQL operations (like a .Where() invocation).</remarks>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static MemberInfo GetMemberInfo<T, TValue>(this Expression<Func<T, TValue>> property)
        {
            Guard.ArgumentNotNull(property, nameof(property));

            // current expression should be MemberExpression in general case
            var currentExpression = property.Body;

            // extract from unary expression
            var unaryExpression = currentExpression as UnaryExpression;
            if (unaryExpression != null)
            {
                currentExpression = unaryExpression.Operand;
            }

            // by this point the expression should be member expression
            var memberExpression = currentExpression as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("MemberExpression cannot be acquired.", nameof(property));
            }

            // return the acquired member info from the member expression
            return memberExpression.Member;
        }

        public static string GetParameterName(this Expression<Func<object>> property)
        {
            // get its name
            return ((MemberExpression)property.Body).Member.Name;
        }

        public static string GetPropertyName<T>(this Expression<Func<T, object>> property)
        {
            // get member expression if it is inside unary expression
            var memberExpression = property.Body is UnaryExpression
                ? ((UnaryExpression)property.Body).Operand
                : property.Body;

            // get its name
            return ((MemberExpression)memberExpression).Member.Name;
        }

        /// <summary>
        /// Combines two expressions with OR logic (return true if expression 1 is true OR expression 2 is true).
        /// </summary>
        /// <typeparam name="T">The type of object being examined in both expressions.</typeparam>
        /// <param name="expr1">The first expression.</param>
        /// <param name="expr2">The second expression, will be combined with the first into a single expression.</param>
        /// <returns>An expression which combines the two given expressions with an OR operation.</returns>
        /// <remarks>This is a handy way to combine expression clauses for Linq-to-SQL operations (like a .Where() invocation).</remarks>
        public static Expression<Func<T, bool>> Or<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}