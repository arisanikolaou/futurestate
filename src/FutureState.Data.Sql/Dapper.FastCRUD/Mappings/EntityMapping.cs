using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Dapper.FastCrud.Validations;

namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    ///     Holds information about table mapped properties for a particular entity type.
    ///     Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public abstract class EntityMapping
    {
        private readonly List<PropertyMapping> _propertyMappings;
        private readonly Dictionary<string, PropertyMapping> _propertyNameMappingsMap;
        private Dictionary<Type, EntityMappingRelationship> _childParentRelationships;
        private volatile bool _isFrozen;
        private Dictionary<Type, EntityMappingRelationship> _parentChildRelationships;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        protected EntityMapping(Type entityType)
        {
            EntityType = entityType;
            TableName = entityType.Name;
            Dialect = OrmConfiguration.DefaultDialect;

            _propertyMappings = new List<PropertyMapping>();
            _propertyNameMappingsMap = new Dictionary<string, PropertyMapping>();
        }

        /// <summary>
        ///     The table associated with the entity.
        /// </summary>
        public string TableName { get; protected set; }

        /// <summary>
        ///     The schema associated with the entity.
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        ///     If the entity mapping was already registered, this flag will return true. You can have multiple mappings which can
        ///     be obtained by cloning this instance.
        /// </summary>
        public bool IsFrozen => _isFrozen;

        /// <summary>
        ///     Current Sql dialect in use for the current entity.
        /// </summary>
        public SqlDialect Dialect { get; protected set; }

        /// <summary>
        ///     Entity type.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        ///     Gets the property mapping asscoiated with the entity.
        /// </summary>
        internal IReadOnlyDictionary<string, PropertyMapping> PropertyMappings => _propertyNameMappingsMap;

        /// <summary>
        ///     Gets all the child-parent relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ChildParentRelationships
            => _childParentRelationships;

        /// <summary>
        ///     Gets all the parent-child relationships.
        /// </summary>
        internal IReadOnlyDictionary<Type, EntityMappingRelationship> ParentChildRelationships
            => _parentChildRelationships;

        /// <summary>
        ///     Freezes changes to the property mappings.
        /// </summary>
        internal void FreezeMapping()
        {
            if (_isFrozen)
                return;

            lock (this)
            {
                if (_isFrozen)
                    return;
                var maxColumnOrder = _propertyMappings.Select(propMapping => propMapping.ColumnOrder).Max();
                foreach (var propMapping in _propertyMappings)
                    if (propMapping.ColumnOrder < 0)
                        propMapping.ColumnOrder = ++maxColumnOrder;

                ConstructChildParentEntityRelationships();
                ConstructParentChildEntityRelationships();

                _isFrozen = true;
            }
        }

        /// <summary>
        ///     Throws an exception if entity mappings cannot be changed.
        /// </summary>
        protected void ValidateState()
        {
            if (IsFrozen)
                throw new InvalidOperationException(
                    "No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
        }

        /// <summary>
        ///     Removes a set of property mappings.
        /// </summary>
        protected void RemoveProperties(IEnumerable<string> paramNames, bool exclude)
        {
            var propNamesMappingsToRemove = new List<string>(_propertyMappings.Count);
            propNamesMappingsToRemove.AddRange(from propMapping in PropertyMappings
                where
                    exclude && !paramNames.Contains(propMapping.Value.PropertyName) ||
                    !exclude && paramNames.Contains(propMapping.Value.PropertyName)
                select propMapping.Key);

            foreach (var propName in propNamesMappingsToRemove)
                RemoveProperty(propName);
        }

        /// <summary>
        ///     Removes a property mapping.
        /// </summary>
        public void RemoveProperty(string propertyName)
        {
            PropertyMapping propertyMapping;
            if (_propertyNameMappingsMap.TryGetValue(propertyName, out propertyMapping))
                if (!_propertyNameMappingsMap.Remove(propertyName) || !_propertyMappings.Remove(propertyMapping))
                    throw new InvalidOperationException($"Failure removing property '{propertyName}'");
        }

        /// <summary>
        ///     Prepares a new property mapping.
        /// </summary>
        public PropertyMapping SetPropertyByName(string propertyName)
        {
            var propDescriptor =
                TypeDescriptor.GetProperties(EntityType)
                    .OfType<PropertyDescriptor>()
                    .Single(propInfo => propInfo.Name == propertyName);

            return SetPropertyByDescriptor(propDescriptor);
        }

        /// <summary>
        ///     Registers a property mapping.
        /// </summary>
        public PropertyMapping SetProperty(PropertyDescriptor property)
        {
            ValidateState();

            return SetPropertyByMapping(new PropertyMapping(this, property));
        }

        /// <summary>
        ///     Registers a property mapping.
        /// </summary>
        public PropertyMapping SetPropertyByDescriptor(PropertyDescriptor property)
        {
            return SetPropertyByMapping(new PropertyMapping(this, property));
        }

        /// <summary>
        ///     Registers a property mapping.
        /// </summary>
        public PropertyMapping SetPropertyByMapping(PropertyMapping propertyMapping)
        {
            Requires.Argument(propertyMapping.EntityMapping == this, nameof(propertyMapping),
                "Unable to add a property mapping that is not assigned to the current entity mapping");
            _propertyMappings.Remove(propertyMapping);
            _propertyMappings.Add(propertyMapping);
            _propertyNameMappingsMap[propertyMapping.PropertyName] = propertyMapping;

            return propertyMapping;
        }

        private void ConstructChildParentEntityRelationships()
        {
            _childParentRelationships = _propertyMappings
                .Where(propertyMapping => propertyMapping.ChildParentRelationship != null)
                .GroupBy(propertyMapping => propertyMapping.ChildParentRelationship.ReferencedEntityType)
                .ToDictionary(
                    groupedRelMappings => groupedRelMappings.Key,
                    groupedRelMappings =>
                    {
                        var referencingEntityPropertyNames = groupedRelMappings
                            .Select(propMapping => propMapping.ChildParentRelationship.ReferencingPropertyName)
                            .Where(propName => !string.IsNullOrEmpty(propName))
                            .Distinct()
                            .ToArray();

                        if (referencingEntityPropertyNames.Length > 1)
                            throw new InvalidOperationException(
                                $"Multiple entity referencing properties were registered for the '{EntityType}' - '{groupedRelMappings.Key}' relationship");

                        var referencingEntityPropertyName = referencingEntityPropertyNames.Length == 0
                            ? null
                            : referencingEntityPropertyNames[0];
                        var referencingEntityPropertyDescriptor = referencingEntityPropertyName == null
                            ? null
                            : TypeDescriptor.GetProperties(EntityType)
                                .OfType<PropertyDescriptor>()
                                .SingleOrDefault(propDescriptor =>
                                    propDescriptor.Name == referencingEntityPropertyName);


                        return new EntityMappingRelationship(groupedRelMappings.Key,
                            groupedRelMappings.OrderBy(propMapping => propMapping.ColumnOrder).ToArray(),
                            referencingEntityPropertyDescriptor);
                    });
        }

        private void ConstructParentChildEntityRelationships()
        {
            _parentChildRelationships = TypeDescriptor.GetProperties(EntityType).OfType<PropertyDescriptor>()
                .Where(propDescriptor =>
                {
                    var propInfo =
#if COREFX
                           propDescriptor.PropertyType.GetTypeInfo();
#else
                        propDescriptor.PropertyType;
#endif
                    return propInfo.IsGenericType && typeof(IEnumerable).IsAssignableFrom(propDescriptor.PropertyType)
                                                  && propDescriptor.PropertyType.GetGenericArguments().Length == 1;
                })
                //.GroupBy(propDescriptor => propDescriptor.PropertyType)
                .ToDictionary(
                    propDescriptor => propDescriptor.PropertyType.GetGenericArguments()[0],
                    propDescriptor =>
                    {
                        // get the keys and order them
                        var keyPropMappings =
                            _propertyMappings.Where(propMapping => propMapping.IsPrimaryKey)
                                .OrderBy(propMapping => propMapping.ColumnOrder)
                                .ToArray();
                        return new EntityMappingRelationship(propDescriptor.PropertyType, keyPropMappings,
                            propDescriptor);
                    });
        }
    }

    /// <summary>
    ///     Holds information about table mapped properties for a particular entity type.
    ///     Multiple instances of such mappings can be active for a single entity type.
    /// </summary>
    public class EntityMapping<TEntity> : EntityMapping
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EntityMapping()
            : base(typeof(TEntity))
        {
        }

        /// <summary>
        ///     Sets the database table associated with your entity.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public EntityMapping<TEntity> SetTableName(string tableName)
        {
            Requires.NotNullOrWhiteSpace(tableName, nameof(tableName));
            ValidateState();

            TableName = tableName;
            return this;
        }

        /// <summary>
        ///     Sets the database schema associated with your entity.
        /// </summary>
        /// <param name="schemaName">Shema name</param>
        public EntityMapping<TEntity> SetSchemaName(string schemaName)
        {
            ValidateState();

            SchemaName = schemaName;
            return this;
        }

        /// <summary>
        ///     You can override the default dialect used for the schema.
        ///     However, if plan on using the same dialect for all your db operations, it's best to use
        ///     <see cref="OrmConfiguration.DefaultDialect" /> instead.
        /// </summary>
        /// <param name="dialect">Sql dialect</param>
        public EntityMapping<TEntity> SetDialect(SqlDialect dialect)
        {
            ValidateState();

            Dialect = dialect;
            return this;
        }

        /// <summary>
        ///     Registers a regular property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> SetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            return SetProperty(property, null);
        }

        /// <summary>
        ///     Sets the mapping options for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        /// <param name="propertySetupFct">A callback which will be called for setting up the property mapping.</param>
        public EntityMapping<TEntity> SetProperty<TProperty>(
            Expression<Func<TEntity, TProperty>> property,
            Action<PropertyMapping> propertySetupFct)
        {
            ValidateState();

            var propName = ((MemberExpression) property.Body).Member.Name;
            var propMapping = SetPropertyByName(propName);
            propertySetupFct?.Invoke(propMapping);
            return this;
        }

        public EntityMapping<TEntity> SetProperty(
            string propertyName,
            Action<PropertyMapping> propertySetupFct)
        {
            ValidateState();

            var propMapping = SetPropertyByName(propertyName);
            propertySetupFct?.Invoke(propMapping);
            return this;
        }

        public EntityMapping<TEntity> SetProperty(
            string propertyName)
        {
            ValidateState();

            SetPropertyByName(propertyName);

            return this;
        }

        /// <summary>
        ///     Returns all the property mappings, optionally filtered by their options.
        /// </summary>
        public PropertyMapping[] GetProperties(params PropertyMappingOptions[] includeFilter)
        {
            return PropertyMappings.Values
                .Where(
                    propInfo =>
                        includeFilter.Length == 0 ||
                        includeFilter.Any(options => (options & propInfo.Options) == options))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        ///     Gives an option for updating all the property mappings, optionally filtered by their options.
        /// </summary>
        public EntityMapping<TEntity> UpdateProperties(Action<PropertyMapping> updateFct,
            params PropertyMappingOptions[] includeFilter)
        {
            foreach (var propMapping in GetProperties(includeFilter))
                updateFct(propMapping);

            return this;
        }

        /// <summary>
        ///     Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public PropertyMapping[] GetPropertiesExcluding(params PropertyMappingOptions[] excludeFilter)
        {
            return PropertyMappings.Values
                .Where(
                    propInfo =>
                        excludeFilter.Length == 0 ||
                        excludeFilter.All(options => (options & propInfo.Options) != options))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        ///     Gives an option for updating all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping> updateFct,
            params PropertyMappingOptions[] excludeFilter)
        {
            foreach (var propMapping in GetPropertiesExcluding(excludeFilter))
                updateFct(propMapping);

            return this;
        }

        /// <summary>
        ///     Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public PropertyMapping[] GetPropertiesExcluding(params string[] propNames)
        {
            return PropertyMappings.Values
                .Where(propInfo => propNames.Length == 0 || !propNames.Contains(propInfo.PropertyName))
                //.OrderBy(propInfo => propInfo.Order)
                .ToArray();
        }

        /// <summary>
        ///     Returns all the property mappings, filtered by an exclusion filter.
        /// </summary>
        public EntityMapping<TEntity> UpdatePropertiesExcluding(Action<PropertyMapping> updateFct,
            params string[] propNames)
        {
            foreach (var propMapping in GetPropertiesExcluding(propNames))
                updateFct(propMapping);

            return this;
        }

        /// <summary>
        ///     Returns property mapping information for a particular property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public PropertyMapping GetProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            var propName = ((MemberExpression) property.Body).Member.Name;
            return GetProperty(propName);
        }

        /// <summary>
        ///     Returns property mapping information for a particular property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public PropertyMapping GetProperty(string propertyName)
        {
            PropertyMapping propertyMapping = null;
            PropertyMappings.TryGetValue(propertyName, out propertyMapping);
            return propertyMapping;
        }

        /// <summary>
        ///     Removes the mapping for a property.
        /// </summary>
        /// <param name="property">Name of the property (e.g. user => user.LastName ) </param>
        public EntityMapping<TEntity> RemoveProperty<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            ValidateState();

            var propName = ((MemberExpression) property.Body).Member.Name;
            RemoveProperties(new[] {propName}, false);
            return this;
        }

        /// <summary>
        ///     Removes the mapping for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveProperty(params string[] propertyName)
        {
            ValidateState();

            RemoveProperties(propertyName, false);
            return this;
        }

        /// <summary>
        ///     Removes all the property mappings with the exception of the provided list.
        /// </summary>
        /// <param name="propertyName">Name of the property (e.g. nameof(User.Name) ) </param>
        public EntityMapping<TEntity> RemoveAllPropertiesExcluding(params string[] propertyName)
        {
            ValidateState();

            RemoveProperties(propertyName, true);
            return this;
        }

        /// <summary>
        ///     Clones the current mapping set, allowing for further modifications.
        /// </summary>
        public EntityMapping<TEntity> Clone()
        {
            var clonedMappings = new EntityMapping<TEntity>()
                .SetSchemaName(SchemaName)
                .SetTableName(TableName)
                .SetDialect(Dialect);
            foreach (
                var clonedPropMapping in
                PropertyMappings.Select(propNameMapping => propNameMapping.Value.Clone(clonedMappings)))
                clonedMappings.SetPropertyByMapping(clonedPropMapping);

            return clonedMappings;
        }
    }
}