using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A well known entity processed within a given flow.
    /// </summary>
    public class FlowEntity
    {
        /// <summary>
        ///     Creates a new flow entity type.
        /// </summary>
        /// <typeparam name="TType">
        ///     The target entiy type to create.
        /// </typeparam>
        /// <returns>
        ///     A new flow entity.
        /// </returns>
        public static FlowEntity Create<TType>()
        {
            return new FlowEntity(typeof(TType));
        }

        /// <summary>
        ///     Creates a new flow entity type based on a given type.
        /// </summary>
        /// <param name="type">The CLR type of the entity.</param>
        public FlowEntity(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            this.AssemblyQualifiedTypeName = type.AssemblyQualifiedName;
            this.DateAdded = DateTime.UtcNow;
            this.EntityTypeId = type.Name;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowEntity()
        {
            // required by serializer
        }

        /// <summary>
        ///     Gets the assembly qualified flow entity. This is the type name of the material form of the entity.
        /// </summary>
        public string AssemblyQualifiedTypeName { get; set; }

        /// <summary>
        ///     Gets the date, in UTC, the flow entity was added.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the entity type id. This id must be unique within a given flow.
        /// </summary>
        public string EntityTypeId { get; set; }
    }
}