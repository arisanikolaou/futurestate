using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.FluentMap.Dommel.Mapping;
using FutureState;

namespace Dapper.Extensions.Linq.Mapper
{
    /// <summary>
    ///     Maps an entity property to its corresponding column in the database.
    /// </summary>
    public class LinqPropertyMap : IPropertyMap
    {
        internal const string LinqBinary = "System.Data.Linq.Binary";

        private static readonly Dictionary<Type, DbType> _typeMap = new Dictionary<Type, DbType>
        {
            {typeof(byte), DbType.Byte},
            {typeof(sbyte), DbType.SByte},
            {typeof(short), DbType.Int16},
            {typeof(ushort), DbType.UInt16},
            {typeof(int), DbType.Int32},
            {typeof(uint), DbType.UInt32},
            {typeof(long), DbType.Int64},
            {typeof(ulong), DbType.UInt64},
            {typeof(float), DbType.Single},
            {typeof(double), DbType.Double},
            {typeof(decimal), DbType.Decimal},
            {typeof(bool), DbType.Boolean},
            {typeof(string), DbType.String},
            {typeof(char), DbType.StringFixedLength},
            {typeof(Guid), DbType.Guid},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(TimeSpan), DbType.Time},
            {typeof(byte[]), DbType.Binary},
            {typeof(byte?), DbType.Byte},
            {typeof(sbyte?), DbType.SByte},
            {typeof(short?), DbType.Int16},
            {typeof(ushort?), DbType.UInt16},
            {typeof(int?), DbType.Int32},
            {typeof(uint?), DbType.UInt32},
            {typeof(long?), DbType.Int64},
            {typeof(ulong?), DbType.UInt64},
            {typeof(float?), DbType.Single},
            {typeof(double?), DbType.Double},
            {typeof(decimal?), DbType.Decimal},
            {typeof(bool?), DbType.Boolean},
            {typeof(char?), DbType.StringFixedLength},
            {typeof(Guid?), DbType.Guid},
            {typeof(DateTime?), DbType.DateTime},
            {typeof(DateTimeOffset?), DbType.DateTimeOffset},
            {typeof(TimeSpan?), DbType.Time},
            {typeof(object), DbType.Object}
        };

        private readonly DommelPropertyMap _innerMap;

        public LinqPropertyMap(DommelPropertyMap map)
        {
            Guard.ArgumentNotNull(map, "map");

            _innerMap = map;

            Type = LookupDbType(map.PropertyInfo.PropertyType);
        }

        /// <summary>
        ///     Gets the column name for the current property.
        /// </summary>
        public string ColumnName => _innerMap.ColumnName;

        /// <summary>
        ///     Gets the key type for the current property.
        /// </summary>
        public KeyType KeyType { get; set; }

        /// <summary>
        ///     Gets the ignore status of the current property. If ignored, the current property will not be included in queries.
        /// </summary>
        public bool Ignored { get; set; }

        /// <summary>
        ///     Gets the read-only status of the current property. If read-only, the current property will not be included in
        ///     INSERT and UPDATE queries.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        ///     Gets the property info for the current property.
        /// </summary>
        public PropertyInfo PropertyInfo => _innerMap.PropertyInfo;

        public DbType Type { get; set; }

        public bool CaseSensitive { get; set; }

        private static DbType LookupDbType(Type type)
        {
            DbType dbType;
            var nullUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullUnderlyingType != null) type = nullUnderlyingType;
            if (type.IsEnum && !_typeMap.ContainsKey(type))
                type = Enum.GetUnderlyingType(type);
            if (_typeMap.TryGetValue(type, out dbType))
                return dbType;
            if (type.FullName == LinqBinary)
                return DbType.Binary;
            if (typeof(IEnumerable).IsAssignableFrom(type))
                return (DbType) (-1);

            return DbType.Object;
        }

        /// <summary>
        ///     Fluently sets the key type of the property.
        /// </summary>
        public LinqPropertyMap Key(KeyType keyType)
        {
            if (Ignored)
                throw new ArgumentException($"'{PropertyInfo.Name}' is ignored and cannot be made a key field. ");

            if (IsReadOnly)
                throw new ArgumentException($"'{PropertyInfo.Name}' is readonly and cannot be made a key field. ");

            KeyType = keyType;
            return this;
        }

        /// <summary>
        ///     Fluently sets the ignore status of the property.
        /// </summary>
        public LinqPropertyMap Ignore()
        {
            if (KeyType != KeyType.NotAKey)
                throw new ArgumentException($"'{PropertyInfo.Name}' is a key field and cannot be ignored.");

            Ignored = true;
            return this;
        }

        /// <summary>
        ///     Fluently sets the read-only status of the property.
        /// </summary>
        public LinqPropertyMap ReadOnly()
        {
            if (KeyType != KeyType.NotAKey)
                throw new ArgumentException($"'{PropertyInfo.Name}' is a key field and cannot be marked readonly.");

            IsReadOnly = true;
            return this;
        }

        public void SetType(DbType type)
        {
            Type = type;
        }
    }
}