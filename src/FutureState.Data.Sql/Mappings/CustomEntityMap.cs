﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dapper;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Mapper;
using Dapper.FluentMap.Dommel.Mapping;
using Humanizer;

// ReSharper disable StaticMemberInGenericType

namespace FutureState.Data.Sql.Mappings
{
    public class CustomEntityMap<TEntity> : DommelEntityMap<TEntity>, IClassMapper<TEntity>
        where TEntity : class
    {
        static readonly Dictionary<Type, KeyType> _propertyKeyMappings;

        static readonly string _defaultTableName;
        static readonly string _defaultSchemaName;
        static readonly PropertyInfo[] _properties;

        static CustomEntityMap()
        {
            if (Attribute.IsDefined(typeof(TEntity), typeof(TableAttribute)))
                _defaultTableName = ((TableAttribute) typeof(TEntity).GetCustomAttribute(typeof(TableAttribute))).Name;

            if (Attribute.IsDefined(typeof(TEntity), typeof(SchemaAttribute)))
                _defaultSchemaName = ((SchemaAttribute) typeof(TEntity).GetCustomAttribute(typeof(SchemaAttribute))).Name;

            _propertyKeyMappings = new Dictionary<Type, KeyType>
            {
                {typeof(byte), KeyType.Identity},
                {typeof(byte?), KeyType.Identity},
                {typeof(sbyte), KeyType.Identity},
                {typeof(sbyte?), KeyType.Identity},
                {typeof(short), KeyType.Identity},
                {typeof(short?), KeyType.Identity},
                {typeof(ushort), KeyType.Identity},
                {typeof(ushort?), KeyType.Identity},
                {typeof(int), KeyType.Identity},
                {typeof(int?), KeyType.Identity},
                {typeof(uint), KeyType.Identity},
                {typeof(uint?), KeyType.Identity},
                {typeof(long), KeyType.Identity},
                {typeof(long?), KeyType.Identity},
                {typeof(ulong), KeyType.Identity},
                {typeof(ulong?), KeyType.Identity},
                {typeof(BigInteger), KeyType.Identity},
                {typeof(BigInteger?), KeyType.Identity},
                {typeof(Guid), KeyType.Guid},
                {typeof(Guid?), KeyType.Guid}
            };

            // get the writable public properties
            _properties = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetSetMethod(false) != null).ToArray();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CustomEntityMap()
        {
            // assume pluralized if not assigned a different name.
            var tableName = _defaultTableName ?? typeof(TEntity).Name.Pluralize();

            LinqPropertyMaps = new List<IPropertyMap>();

            ToSchema(_defaultSchemaName);

            // cascade table name
            ToTable(tableName);

            // dapper type mapping wrap into custom type map
            var customTypeMap = new CustomTypeMap<TEntity>(SqlMapper.GetTypeMap(typeof(TEntity)));

            // add all other properties properties
            _properties.Each(m =>
            {
                bool ignore = m.GetCustomAttributes(typeof(NotMappedAttribute)).Any();

                bool isKey = m.GetCustomAttributes(typeof(KeyAttribute)).Any();

                // not already assigned
                var columnName =
                    m.GetCustomAttributes(typeof(ColumnAttribute))
                        .Select(q => ((ColumnAttribute)q).Name)
                        .FirstOrDefault() ?? m.Name;

                var map = new DommelPropertyMap(m);
                map.ToColumn(columnName);

                var linkMap = new LinqPropertyMap(map);

                if (isKey || m.Name == "Id")
                {
                    var keyAssignmentType = _propertyKeyMappings.ContainsKey(linkMap.PropertyInfo.PropertyType)
                        ? _propertyKeyMappings[linkMap.PropertyInfo.PropertyType]
                        : KeyType.Assigned;

                    map.IsKey();
                    linkMap.Key(keyAssignmentType);
                }

                if (ignore)
                    map.Ignore();

                PropertyMaps.Add(map);

                //cascade column mapping to dapper property maps
                customTypeMap.Map(m.Name, columnName);

                linkMap.Key(KeyType.Assigned);
                this.LinqPropertyMaps.Add(linkMap);
            });
        }


        /// <summary>
        ///     Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        ///     A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IPropertyMap> LinqPropertyMaps { get; }

        /// <summary>
        ///     Gets the entity type.
        /// </summary>
        public Type EntityType => typeof(TEntity);

        /// <summary>
        ///     Gets the entity's schema type.
        /// </summary>
        /// <param name="schemaName"></param>
        public void ToSchema(string schemaName)
        {
            SchemaName = schemaName;
        }
    }
}