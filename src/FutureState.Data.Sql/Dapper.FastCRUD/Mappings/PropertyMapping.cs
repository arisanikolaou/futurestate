using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper.FastCrud.Validations;

namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    ///     Reeturns mapping information held for a particular property.
    /// </summary>
    public class PropertyMapping
    {
        private PropertyMappingRelationship _childParentRelationship;
        private int _columnOrder = -1;
        private string _databaseColumnName;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        internal PropertyMapping(EntityMapping entityMapping, PropertyDescriptor descriptor)
        {
            Options = PropertyMappingOptions.None;
            _databaseColumnName = descriptor.Name;

            EntityMapping = entityMapping;
            Descriptor = descriptor;
        }

        /// <summary>
        ///     Gets or sets a child-parent relationship.
        /// </summary>
        public PropertyMappingRelationship ChildParentRelationship
        {
            get { return _childParentRelationship; }
            set
            {
                ValidateState();
                _childParentRelationship = value;
            }
        }

        /// <summary>
        ///     Gets the entity mapping this property mapping is attached to.
        /// </summary>
        public EntityMapping EntityMapping { get; }

        /// <summary>
        ///     Gets or sets a flag indicating the property is mapped to a primary key.
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return (Options & PropertyMappingOptions.KeyProperty) == PropertyMappingOptions.KeyProperty; }
            set
            {
                ValidateState();

                if (value)
                {
                    Options |= PropertyMappingOptions.KeyProperty;
                    IsExcludedFromUpdates = true;
                }
                else
                {
                    Options &= ~PropertyMappingOptions.KeyProperty;
                }
            }
        }

        /// <summary>
        ///     Gets or sets a flag indicating the property is refreshed after an INSERT.
        /// </summary>
        public bool IsRefreshedOnInserts
        {
            get
            {
                return (Options & PropertyMappingOptions.RefreshPropertyOnInserts) ==
                       PropertyMappingOptions.RefreshPropertyOnInserts;
            }
            set
            {
                ValidateState();

                if (value)
                    Options |= PropertyMappingOptions.RefreshPropertyOnInserts;
                else
                    Options &= ~PropertyMappingOptions.RefreshPropertyOnInserts;
            }
        }

        /// <summary>
        ///     Gets or sets a flag indicating the property is refreshed after an UPDATE.
        /// </summary>
        public bool IsRefreshedOnUpdates
        {
            get
            {
                return (Options & PropertyMappingOptions.RefreshPropertyOnUpdates) ==
                       PropertyMappingOptions.RefreshPropertyOnUpdates;
            }
            set
            {
                ValidateState();

                if (value)
                    Options |= PropertyMappingOptions.RefreshPropertyOnUpdates;
                else
                    Options &= ~PropertyMappingOptions.RefreshPropertyOnUpdates;
            }
        }

        /// <summary>
        ///     Gets or sets a flag that indicates the curent property will be excluded from updates.
        /// </summary>
        public bool IsExcludedFromInserts
        {
            get
            {
                return (Options & PropertyMappingOptions.ExcludedFromInserts) ==
                       PropertyMappingOptions.ExcludedFromInserts;
            }
            set
            {
                ValidateState();

                if (value)
                    Options |= PropertyMappingOptions.ExcludedFromInserts;
                else
                    Options &= ~PropertyMappingOptions.ExcludedFromInserts;
            }
        }

        /// <summary>
        ///     Gets or sets a flag that indicates the curent property will be excluded from updates.
        /// </summary>
        public bool IsExcludedFromUpdates
        {
            get
            {
                return (Options & PropertyMappingOptions.ExcludedFromUpdates) ==
                       PropertyMappingOptions.ExcludedFromUpdates;
            }
            set
            {
                ValidateState();

                if (value)
                    Options |= PropertyMappingOptions.ExcludedFromUpdates;
                else
                    Options &= ~PropertyMappingOptions.ExcludedFromUpdates;
            }
        }

        /// <summary>
        ///     Gets or sets the database column name.
        /// </summary>
        public string DatabaseColumnName
        {
            get { return _databaseColumnName; }
            set
            {
                ValidateState();

                Requires.NotNullOrEmpty(value, nameof(DatabaseColumnName));
                _databaseColumnName = value;
            }
        }

        /// <summary>
        ///     Gets the property descriptor of the property attached to the entity type.
        /// </summary>
        public PropertyDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the property name.
        /// </summary>
        public string PropertyName => Descriptor.Name;

        /// <summary>
        ///     Gets or sets the full set of options.
        /// </summary>
        internal PropertyMappingOptions Options { get; private set; }

        /// <summary>
        ///     Gets or sets the column order, normally used for matching foreign keys with the primary composite keys.
        /// </summary>
        public int ColumnOrder
        {
            get { return _columnOrder; }
            set
            {
                ValidateState();

                _columnOrder = value;
            }
        }

        /// <summary>
        ///     Sets up a foreign key relationship with another entity.
        /// </summary>
        /// <typeparam name="TRelatedEntityType">Foreign entity type.</typeparam>
        /// <param name="referencingEntityPropertyName">
        ///     The name of the property on the current entity that would hold the
        ///     referenced entity when instructed to do so in a JOIN statement.
        /// </param>
        public PropertyMapping SetChildParentRelationship<TRelatedEntityType>(string referencingEntityPropertyName)
        {
            return SetChildParentRelationship(typeof(TRelatedEntityType), referencingEntityPropertyName);
        }

        /// <summary>
        ///     Sets up a foreign key relationship with another entity.
        /// </summary>
        /// <param name="relatedEntityType">Foreign entity type.</param>
        /// <param name="referencingEntityPropertyName">
        ///     The name of the property on the current entity that would hold the
        ///     referenced entity when instructed to do so in a JOIN statement.
        /// </param>
        internal PropertyMapping SetChildParentRelationship(Type relatedEntityType, string referencingEntityPropertyName)
        {
            ChildParentRelationship = new PropertyMappingRelationship(relatedEntityType, referencingEntityPropertyName);
            return this;
        }

        /// <summary>
        ///     Removes a parent-child relationship.
        /// </summary>
        public PropertyMapping RemoveChildParentRelationship()
        {
            ChildParentRelationship = null;
            return this;
        }

        /// <summary>
        ///     Marks the property as primary key.
        /// </summary>
        public PropertyMapping SetPrimaryKey(bool isPrimaryKey = true)
        {
            IsPrimaryKey = isPrimaryKey;
            return this;
        }

        /// <summary>
        ///     Indicates that the property is mapped to a database generated field.
        ///     This does not cover default values generated by the database (please use <see cref="ExcludeFromInserts" /> and
        ///     <see cref="RefreshOnInserts" /> for this scenario).
        /// </summary>
        public PropertyMapping SetDatabaseGenerated(DatabaseGeneratedOption option)
        {
            switch (option)
            {
                case DatabaseGeneratedOption.Computed:
                    IsExcludedFromInserts = true;
                    IsExcludedFromUpdates = true;
                    IsRefreshedOnInserts = true;
                    IsRefreshedOnUpdates = true;
                    break;
                case DatabaseGeneratedOption.Identity:
                    IsExcludedFromInserts = true;
                    IsExcludedFromUpdates = true;
                    IsRefreshedOnInserts = true;
                    IsRefreshedOnUpdates = false;
                    break;
                case DatabaseGeneratedOption.None:
                    IsExcludedFromInserts = false;
                    IsExcludedFromUpdates = false;
                    IsRefreshedOnInserts = false;
                    IsRefreshedOnUpdates = false;
                    break;
                default:
                    throw new NotSupportedException($"Option {option} is not supported.");
            }
            return this;
        }

        /////// <summary>
        /////// Gets or sets a flag indicating the property is mapped to a database generated field.
        /////// </summary>
        ////public bool IsDatabaseGenerated
        ////{
        ////    get
        ////    {
        ////        return (_options & PropertyMappingOptions.DatabaseGeneratedProperty) == PropertyMappingOptions.DatabaseGeneratedProperty;
        ////    }
        ////    set
        ////    {
        ////        this.ValidateState();

        ////        if (value)
        ////        {
        ////            _options |= PropertyMappingOptions.DatabaseGeneratedProperty;
        ////            this.IsExcludedFromInserts = true;
        ////        }
        ////        else
        ////        {
        ////            _options &= ~PropertyMappingOptions.DatabaseGeneratedProperty;
        ////        }
        ////    }
        ////}

        /// <summary>
        ///     Sets the column order, normally used for matching foreign keys with the primary composite keys.
        /// </summary>
        public PropertyMapping SetColumnOrder(int columnOrder)
        {
            ColumnOrder = columnOrder;
            return this;
        }

        /// <summary>
        ///     Indicates that the property gets refreshed on INSERTs.
        /// </summary>
        public PropertyMapping RefreshOnInserts(bool refreshOnInsert = true)
        {
            IsRefreshedOnInserts = refreshOnInsert;
            return this;
        }

        /// <summary>
        ///     Indicates that the property gets refreshed on UPDATEs.
        /// </summary>
        public PropertyMapping RefreshOnUpdates(bool refreshOnUpdate = true)
        {
            IsRefreshedOnUpdates = refreshOnUpdate;
            return this;
        }

        /// <summary>
        ///     The property will be included in insert operations.
        /// </summary>
        public PropertyMapping IncludeInInserts()
        {
            IsExcludedFromInserts = false;
            return this;
        }

        /// <summary>
        ///     The property will be excluded from update operations.
        /// </summary>
        public PropertyMapping ExcludeFromInserts()
        {
            IsExcludedFromInserts = true;
            return this;
        }

        /// <summary>
        ///     The property will be included in update operations.
        /// </summary>
        public PropertyMapping IncludeInUpdates()
        {
            IsExcludedFromUpdates = false;
            return this;
        }

        /// <summary>
        ///     The property will be excluded from update operations.
        /// </summary>
        public PropertyMapping ExcludeFromUpdates()
        {
            IsExcludedFromUpdates = true;
            return this;
        }

        /// <summary>
        ///     Sets the database column name.
        /// </summary>
        public PropertyMapping SetDatabaseColumnName(string dbColumnName)
        {
            DatabaseColumnName = dbColumnName;
            return this;
        }

        /// <summary>
        ///     Removes the current property mapping.
        /// </summary>
        public void Remove()
        {
            ValidateState();

            EntityMapping.RemoveProperty(PropertyName);
        }

        internal PropertyMapping Clone(EntityMapping newEntityMapping)
        {
            var clonedPropertyMapping = new PropertyMapping(newEntityMapping, Descriptor)
            {
                Options = Options,
                _childParentRelationship =
                    _childParentRelationship == null
                        ? null
                        : new PropertyMappingRelationship(_childParentRelationship.ReferencedEntityType,
                            _childParentRelationship.ReferencingPropertyName),
                _databaseColumnName = _databaseColumnName,
                _columnOrder = _columnOrder
            };
            return clonedPropertyMapping;
        }

        protected bool Equals(PropertyMapping other)
        {
            return EntityMapping.Equals(other.EntityMapping) && PropertyName.Equals(other.PropertyName);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((PropertyMapping) obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            return PropertyName.GetHashCode();
        }

        public static bool operator ==(PropertyMapping left, PropertyMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyMapping left, PropertyMapping right)
        {
            return !Equals(left, right);
        }

        private void ValidateState()
        {
            if (EntityMapping.IsFrozen)
                throw new InvalidOperationException(
                    "No further modifications are allowed for this entity mapping. Please clone the entity mapping instead.");
        }
    }
}