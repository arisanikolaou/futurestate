using System;
using Dapper.FastCrud.Mappings;

namespace Dapper.FastCrud.EntityDescriptors
{
    /// <summary>
    ///     Basic entity descriptor, holding entity mappings for a specific entity type.
    /// </summary>
    internal abstract class EntityDescriptor
    {
        // this can be set by the developer, prior to making any sql statement calls
        // alternatively it is set by us on first usage
        private volatile EntityMapping _defaultEntityMapping;

        protected EntityDescriptor(Type entityType)
        {
            EntityType = entityType;
        }

        /// <summary>
        ///     Returns the default entity mapping.
        /// </summary>
        public EntityMapping DefaultEntityMapping
        {
            get => _defaultEntityMapping;
            set => _defaultEntityMapping = value;
        }

        /// <summary>
        ///     Gets the associated entity type.
        /// </summary>
        public Type EntityType { get; }
    }
}