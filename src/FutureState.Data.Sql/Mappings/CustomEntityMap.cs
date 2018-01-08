using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using Dapper;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Mapper;
using Dapper.FastCrud;
using Dapper.FastCrud.Mappings;
using Dapper.FluentMap.Dommel.Mapping;
using Humanizer;

// ReSharper disable StaticMemberInGenericType

namespace FutureState.Data.Sql.Mappings
{
    public class CustomEntityMap<TEntity> : DommelEntityMap<TEntity>, IClassMapper<TEntity>
        where TEntity : class
    {
        private static readonly Dictionary<Type, KeyType> _propertyKeyMappings;

        private static readonly string _defaultTableName;
        private static readonly string _defaultSchemaName;
        private static readonly PropertyInfo[] _properties;
        private readonly EntityMapping<TEntity> _fastCrudReg;
        private static readonly PropertyDescriptor[] _propertyDescriptors;
        private static readonly string _tableName;
        private static readonly CustomTypeMap<TEntity> _customTypeMap;

        static CustomEntityMap()
        {
            if (Attribute.IsDefined(typeof(TEntity), typeof(TableAttribute)))
                _defaultTableName = ((TableAttribute) typeof(TEntity).GetCustomAttribute(typeof(TableAttribute))).Name;

            if (Attribute.IsDefined(typeof(TEntity), typeof(SchemaAttribute)))
                _defaultSchemaName = ((SchemaAttribute) typeof(TEntity).GetCustomAttribute(typeof(SchemaAttribute)))
                    .Name;

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
            _properties = typeof(TEntity)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetSetMethod(false) != null)
                // ignore
                .Where(m => !m.GetCustomAttributes(typeof(NotMappedAttribute)).Any())
                .Where(m => !m.GetCustomAttributes(typeof(IgnoreDataMemberAttribute)).Any())
                .ToArray();

            var validProperties = _properties.Select(m => m.Name).Distinct().ToList();

            var descriptors = TypeDescriptor.GetProperties(typeof(TEntity));

            _propertyDescriptors = descriptors
                .OfType<PropertyDescriptor>()
                .Where(m => validProperties.Contains(m.Name))
                .ToArray();

            _tableName = _defaultTableName ?? typeof(TEntity).Name.Pluralize();

            // dapper type mapping wrap into custom type map
            var typeMap = SqlMapper.GetTypeMap(typeof(TEntity));
            _customTypeMap = new CustomTypeMap<TEntity>(typeMap);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CustomEntityMap()
        {
            LinqPropertyMaps = new List<IPropertyMap>();

            ToSchema(_defaultSchemaName);

            // cascade table name
            ToTable(_tableName);

            // fast crud registration
            this._fastCrudReg = OrmConfiguration.RegisterEntity<TEntity>()
                .SetTableName(_tableName);

            if (_propertyDescriptors.Length == 0)
                throw new InvalidOperationException("Entity contains no valid properties.");

            foreach (var descriptor in _propertyDescriptors)
                _fastCrudReg.SetProperty(descriptor);

            // add all other properties properties
            foreach(var m in _properties)
            {
                var isKey = m.GetCustomAttributes(typeof(KeyAttribute)).Any();

                // not already assigned
                var columnName =
                    m.GetCustomAttributes(typeof(ColumnAttribute))
                        .Select(q => ((ColumnAttribute) q).Name)
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

                    if (m.GetCustomAttributes(typeof(DatabaseGeneratedAttribute))
                        .FirstOrDefault() is DatabaseGeneratedAttribute dbGenerated)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        // ReSharper disable once UseNullPropagation
                        if (dbGenerated != null)
                        {
                            if (dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                            {
                                map.IsIdentity();

                                _fastCrudReg
                                    .SetProperty(m.Name,
                                        prop => prop.SetPrimaryKey()
                                            .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));
                            }
                        }
                    }
                    else
                    {
                        //assume database generated
                        _fastCrudReg
                            .SetProperty(m.Name,
                                prop => prop.SetPrimaryKey()
                                    .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));

                    }

                    linkMap.Key(keyAssignmentType);
                }

                PropertyMaps.Add(map);

                //cascade column mapping to dapper property maps
                _customTypeMap.Map(m.Name, columnName);

                linkMap.Key(KeyType.Assigned);

                LinqPropertyMaps.Add(linkMap);
            }
        }

        /// <summary>
        ///     Marks a given column as identity generated.
        /// </summary>
        public void SetIdentityGenerated<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            _fastCrudReg
                .SetProperty(property,
                    prop => prop.SetPrimaryKey()
                        .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));
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