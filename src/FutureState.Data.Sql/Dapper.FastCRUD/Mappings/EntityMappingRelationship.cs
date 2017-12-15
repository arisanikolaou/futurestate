﻿using System;
using System.ComponentModel;

namespace Dapper.FastCrud.Mappings
{
    /// <summary>
    ///     Gives information about a relationship between two entities.
    /// </summary>
    internal class EntityMappingRelationship
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public EntityMappingRelationship(
            Type referencedEntityType,
            PropertyMapping[] referencingKeyProperties,
            PropertyDescriptor referencingEntityProperty = null)
        {
            ReferencingKeyProperties = referencingKeyProperties;
            ReferencingEntityProperty = referencingEntityProperty;
            ReferencedEntityType = referencedEntityType;
        }

        /// <summary>
        ///     The main entity properties through which the relationship is established.
        /// </summary>
        public PropertyMapping[] ReferencingKeyProperties { get; }

        /// <summary>
        ///     The property representing the entity the relationship reffers to. It can be null.
        /// </summary>
        public PropertyDescriptor ReferencingEntityProperty { get; }

        /// <summary>
        ///     Gets the referenced entity type.
        /// </summary>
        public Type ReferencedEntityType { get; }
    }
}