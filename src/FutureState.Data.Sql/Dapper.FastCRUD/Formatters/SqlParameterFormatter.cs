using Dapper.FastCrud.EntityDescriptors;
using Dapper.FastCrud.Mappings;
using System;
using System.Globalization;

namespace Dapper.FastCrud.Formatters
{
    /// <summary>
    ///     Deferrs the resolution of a parameter until the query is ready to produce the SQL.
    /// </summary>
    internal class SqlParameterFormatter : IFormattable
    {
        /// <summary>
        ///     Constructor used when the entity type is set to the query's main one.
        /// </summary>
        public SqlParameterFormatter(SqlParameterElementType elementType, string parameterValue,
            EntityMapping entityMappingOverride)
            : this(elementType, parameterValue, null, entityMappingOverride)
        {
        }

        /// <summary>
        ///     Constructor used by entity type aware formatters.
        /// </summary>
        protected SqlParameterFormatter(SqlParameterElementType elementType, string parameterValue, Type entityType,
            EntityMapping entityMappingOverride)
        {
            ElementType = elementType;
            ParameterValue = parameterValue;
            EntityType = entityType;
            EntityMappingOverride = entityMappingOverride;
        }

        /// <summary>
        ///     Gets the original value of the parameter.
        /// </summary>
        public string ParameterValue { get; }

        /// <summary>
        ///     Gets the SQL representation of the parameter.
        /// </summary>
        public SqlParameterElementType ElementType { get; }

        /// <summary>
        ///     Gets the type of the entity attached to the resolver.
        ///     If NULL, the resolver was created for the main entity.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        ///     Optional entity mapping .
        /// </summary>
        protected EntityMapping EntityMappingOverride { get; }

        /// <summary>
        ///     Formats the value of the current instance using the specified format.
        /// </summary>
        /// <returns>
        ///     The value of the current instance in the specified format.
        /// </returns>
        /// <param name="format">
        ///     The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format
        ///     defined for the type of the <see cref="T:System.IFormattable" /> implementation.
        /// </param>
        /// <param name="formatProvider">
        ///     The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to
        ///     obtain the numeric format information from the current locale setting of the operating system.
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            ISqlBuilder sqlBuilder;

            SqlStatementFormatter sqlQueryFormatter;

            // is it our formatter?
            if ((sqlQueryFormatter = formatProvider as SqlStatementFormatter) != null)
                if (EntityType == null || EntityType == sqlQueryFormatter.MainEntityType)
                    if (EntityMappingOverride == null || EntityMappingOverride == sqlQueryFormatter.MainEntityMapping)
                        sqlBuilder = sqlQueryFormatter.MainEntitySqlBuilder;
                    else
                        sqlBuilder = GetSqlBuilder(sqlQueryFormatter.MainEntityDescriptor, EntityMappingOverride);
                else
                    sqlBuilder = GetSqlBuilder(null, EntityMappingOverride);
            else
                sqlBuilder = GetSqlBuilder(null, EntityMappingOverride);

            if (sqlBuilder == null)
                throw new InvalidOperationException("Not enough information is available in the current context.");

            switch (ElementType)
            {
                case SqlParameterElementType.Column:
                    return sqlBuilder.GetColumnName(ParameterValue);

                case SqlParameterElementType.Table:
                    return sqlBuilder.GetTableName();

                case SqlParameterElementType.TableAndColumn:
                    return string.Format(CultureInfo.InvariantCulture, "{0}.{1}", sqlBuilder.GetTableName(),
                        sqlBuilder.GetColumnName(ParameterValue));

                case SqlParameterElementType.Identifier:
                    return sqlBuilder.GetDelimitedIdentifier(ParameterValue);

                default:
                    throw new InvalidOperationException($"Unknown SQL element type {ElementType}");
            }
        }

        /// <summary>
        ///     If overridden, returns the sql builder associated with the optional entity descriptor and entity mapping.
        ///     Note: Any or all the parameters can be <c>NULL</c>
        /// </summary>
        protected virtual ISqlBuilder GetSqlBuilder(EntityDescriptor entityDescriptor, EntityMapping entityMapping)
        {
            return null;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            // this really shouldn't be called directly.
            return ToString(null, null);
        }
    }
}