using Dapper;
using Dapper.Extensions.Linq.Core.Attributes;
using Dapper.Extensions.Linq.Core.Enums;
using Dapper.Extensions.Linq.Core.Mapper;
using Dapper.Extensions.Linq.Mapper;
using Dapper.FastCrud;
using Dapper.FastCrud.Mappings;
using Dapper.FluentMap.Dommel.Mapping;
using Humanizer;
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

// ReSharper disable StaticMemberInGenericType

namespace FutureState.Data.Sql.Mappings
{
    /// <summary>
    ///     Creates a custom type map that supports both fast crud and dapper type maps.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to map.</typeparam>
    public class CustomEntityMap<TEntity> : DommelEntityMap<TEntity>, IClassMapper<TEntity>
        where TEntity : class
    {
        private static readonly Dictionary<Type, KeyType> _propertyKeyMappings;
        private static readonly IList<DommelPropertyMap> _propertyMaps;
        private static readonly IList<IPropertyMap> _linqPropertyMaps;
        private static readonly string _defaultTableName;
        private static readonly string _defaultSchemaName;
        private static readonly PropertyInfo[] _properties;
        private static readonly EntityMapping<TEntity> _fastCrudMap;
        private static readonly PropertyDescriptor[] _propertyDescriptors;
        private static readonly string _tableName;
        private static readonly CustomTypeMap<TEntity> _customTypeMap;

        static CustomEntityMap()
        {
            if (Attribute.IsDefined(typeof(TEntity), typeof(TableAttribute)))
                _defaultTableName = ((TableAttribute)typeof(TEntity).GetCustomAttribute(typeof(TableAttribute)))
                    .Name;

            if (Attribute.IsDefined(typeof(TEntity), typeof(SchemaAttribute)))
                _defaultSchemaName = ((SchemaAttribute)typeof(TEntity).GetCustomAttribute(typeof(SchemaAttribute)))
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

            // fast crud registration
            _fastCrudMap = OrmConfiguration.RegisterEntity<TEntity>()
                .SetTableName(_tableName);

            AdaptToFastCrud(_fastCrudMap, _customTypeMap);

            _linqPropertyMaps = new List<IPropertyMap>();

            _propertyMaps = new List<DommelPropertyMap>();

            // add all other properties properties
            foreach (PropertyInfo property in _properties)
            {
                bool isKey = property.GetCustomAttributes(typeof(KeyAttribute)).Any();

                // not already assigned
                string columnName =
                    property.GetCustomAttributes(typeof(ColumnAttribute))
                        .Select(q => ((ColumnAttribute)q).Name)
                        .FirstOrDefault() ?? property.Name; // default to property name

                var map = new DommelPropertyMap(property);
                map.ToColumn(columnName);

                var linkMap = new LinqPropertyMap(map);

                if (isKey || property.Name == "Id")
                {
                    var keyAssignmentType = _propertyKeyMappings.ContainsKey(linkMap.PropertyInfo.PropertyType)
                        ? _propertyKeyMappings[linkMap.PropertyInfo.PropertyType]
                        : KeyType.Assigned;

                    map.IsKey();

                    SetupIfIdentity(property, map);

                    linkMap.Key(keyAssignmentType);
                }

                _propertyMaps.Add(map);

                //cascade column mapping to dapper property maps
                _customTypeMap.Map(property.Name, columnName);

                linkMap.Key(KeyType.Assigned);

                _linqPropertyMaps.Add(linkMap);
            }
        }

        private static void AdaptToFastCrud(EntityMapping<TEntity> mapping, SqlMapper.ITypeMap entityMap)
        {
            var currentConventions = OrmConfiguration.Conventions;

            foreach (PropertyDescriptor propDescriptor in _propertyDescriptors)
            {
                SqlMapper.IMemberMap entityMember = entityMap.GetMember(propDescriptor.Name);

                PropertyMapping propMapping = mapping
                    .SetPropertyByMapping(new PropertyMapping(mapping, propDescriptor));

                if (entityMember != null)
                    propMapping.DatabaseColumnName = entityMember.ColumnName;

                currentConventions.ConfigureEntityPropertyMapping(propMapping);
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public CustomEntityMap()
        {
            if (_propertyDescriptors.Length == 0)
                throw new InvalidOperationException("Entity contains no valid properties.");

            SchemaName = _defaultSchemaName;

            // cascade table name
            ToTable(_tableName);

            // property maps for fast crud as well as dapper
            FillPropertyMaps();
        }

        private void FillPropertyMaps()
        {
            foreach (var map in _propertyMaps)
                this.PropertyMaps.Add(map);

            foreach (var linqPropertyMap in _linqPropertyMaps)
                this.LinqPropertyMaps.Add(linqPropertyMap);
        }

        private static void SetupIfIdentity(PropertyInfo property, DommelPropertyMap map)
        {
            if (property.GetCustomAttributes(typeof(DatabaseGeneratedAttribute))
                .FirstOrDefault() is DatabaseGeneratedAttribute dbGenerated)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // ReSharper disable once UseNullPropagation
                if (dbGenerated == null)
                    return;

                if (dbGenerated.DatabaseGeneratedOption != DatabaseGeneratedOption.Identity)
                    return;

                map.IsIdentity();

                _fastCrudMap
                    .SetProperty(property.Name,
                        prop => prop.SetPrimaryKey()
                            .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));
            }
            else
            {
                //assume database generated and pk
                _fastCrudMap
                    .SetProperty(property.Name,
                        prop => prop.SetPrimaryKey()
                            .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));
            }
        }

        /// <summary>
        ///     Marks a given column as identity generated.
        /// </summary>
        public void SetIdentityGenerated<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            _fastCrudMap
                .SetProperty(property,
                    prop => prop.SetPrimaryKey()
                        .SetDatabaseGenerated(DatabaseGeneratedOption.Identity));
        }

        /// <summary>
        ///     Gets or sets the schema to use when referring to the corresponding table name in the database.
        /// </summary>
        public string SchemaName { get; }

        /// <summary>
        ///     A collection of properties that will map to columns in the database table.
        /// </summary>
        public IList<IPropertyMap> LinqPropertyMaps { get; } = new List<IPropertyMap>();

        /// <summary>
        ///     Gets the entity type.
        /// </summary>
        public Type EntityType => typeof(TEntity);
    }
}