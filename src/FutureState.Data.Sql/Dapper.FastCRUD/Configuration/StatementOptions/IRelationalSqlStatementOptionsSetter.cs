using System;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

namespace Dapper.FastCrud.Configuration.StatementOptions
{
    /// <summary>
    ///     Statement options for entity relationships
    /// </summary>
    public interface IRelationalSqlStatementOptionsSetter<TReferencingEntity, TStatementOptionsBuilder>
    {
        /// <summary>
        ///     Includes a referred entity into the query. The relationship must be set up prior to calling this method.
        /// </summary>
        TStatementOptionsBuilder Include<TReferredEntity>(
            Action<ISqlRelationOptionsBuilder<TReferredEntity>> options = null);
    }
}