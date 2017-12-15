using FutureState.Data;
using System.ComponentModel.DataAnnotations;
using FutureState.Specifications;

namespace FutureState.Domain
{
    /// <summary>
    ///     Describes the lifecycle phases of an architectural asset (model).
    /// </summary>
    public class LifeCycle : IEntityMutableKey<string>
    {
        [Required]
        [Key]
        [StringLength(200)]
        [NotEmpty("Id", ErrorMessage = "Id cannot be empty.")]
        public string Id { get; set; }

        // required by serializer to be public .. todo: make public or protected internal
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public LifeCycle()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="id">The life cycel state, e.g. invest, exit, maintain etc.</param>
        public LifeCycle(string id)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(id, nameof(id));

            Id = id;
        }

        public const string Invest = "Invest";
        public const string Exit = "Exit";
        public const string Maintain = "Maintain";
        public const string Research = "Research";
    }
}