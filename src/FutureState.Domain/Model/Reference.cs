using FutureState.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     An external reference to describe a given architectural asset.
    /// </summary>
    public class Reference : IEntityMutableKey<Guid>, IDesignArtefact 
    {
        /// <summary>
        ///     Gets/set the id of the reference.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets/sets the architectural scenario the reference belongs to. If null belongs to
        ///     a default scenario.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets a description of the reference link
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        ///     Gets the link address, e.g. http://confluence/doc
        /// </summary>
        [StringLength(100)]
        public string Link { get; set; }

        /// <summary>
        ///     The entity associated with the link.
        /// </summary>
        public Guid ReferenceId { get; set; }

        // required for serializer
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Reference()
        {
            //required by the serializer
        }

        /// <summary>
        ///     Creates a reference associated to a design artefact bound to the same design scenario.
        /// </summary>
        public Reference(IDesignArtefact container, string link, string description = null) : this(container.Id, link, description)
        {
            this.ReferenceId = container.Id;
            this.ScenarioId = container.ScenarioId;
        }

        /// <summary>
        ///     Creates a references associated to a given reference id.
        /// </summary>
        public Reference(Guid referenceId, string link, string description = null)
        {
            this.Id = SeqGuid.Create();

            this.ReferenceId = referenceId;
            this.Link = link;
            this.Description = description ?? "";
        }
    }
}