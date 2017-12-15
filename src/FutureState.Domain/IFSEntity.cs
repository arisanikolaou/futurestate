using System;
using System.ComponentModel.DataAnnotations;
using FutureState.Specifications;

namespace FutureState.Domain
{
    public interface IFSEntity
    {
        Guid Id { get; }

        string DisplayName { get; }

        string Description { get; }
    }

    /// <summary>
    ///     Base entity definition for an enterprise asset or model.
    /// </summary>
    public abstract class FSEntity : IFSEntity
    {
        /// <summary>
        ///     Gets the software model id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets the display name of the interface.
        /// </summary>
        [StringLength(100)]
        [NotEmpty("DisplayName", ErrorMessage = "Display Name is required.")]
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets the description of the port.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
    }
}