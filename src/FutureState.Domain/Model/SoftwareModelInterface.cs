using FutureState.Data;
using FutureState.Specifications;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Endpoint other information systems can either send or receive information from exposed by software. 
    /// </summary>
    /// <remarks>
    ///     Data is exchanged by a given protocol.
    /// </remarks>
    public class SoftwareModelInterface : IEntityMutableKey<Guid>, IFSEntity, IDesignArtefact
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
        [NotEmpty("Interface name is required.")]
        public string DisplayName { get; set; }

        /// <summary>
        ///     Gets/sets the scenario the software model interface belongs to.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the description of the port.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        ///     Gets the date the model was created.
        /// </summary>
        [NotEmptyDate("Date created is required.")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the software model id that exposes the interface.
        /// </summary>
        [NotEmpty("Software model id is required.")]
        public Guid SoftwareModelId { get; set; }

        /// <summary>
        ///     Gets the schema or protocol that is exposed through the interface. This is basically
        ///     the interface schema or contract identifier.
        /// </summary>
        public Guid? ProtocolId { get; set; }

        /// <summary>
        ///     Gets/sets the external id for the software model interface.
        /// </summary>
        [StringLength(150)]
        [NotEmpty("External id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the explicitly assigned sensitivity level of the data exchanged over the interface.
        /// </summary>
        [Range(0,10)]
        public int SensitivityLevel { get; set; }

        /// <summary>
        ///     Gets the name of the user that last modified the record.
        /// </summary>
        [StringLength(150)]
        [NotEmpty("User name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SoftwareModelInterface()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SoftwareModelInterface(SoftwareModel model, string externalId, string displayName, string description = null)
        {
            Guard.ArgumentNotNull(model, nameof(model));

            this.Id = SeqGuid.Create();

            this.ExternalId = externalId;
            this.DisplayName = displayName;
            this.DateCreated = DateTime.UtcNow;
            this.Description = description ?? "";
            this.ScenarioId = model.ScenarioId;
            this.SoftwareModelId = model.Id;
            this.UserName = "";
        }

        /// <summary>
        ///     Model service extensions.
        /// </summary>
        public DomainServices Services { get; private set; } // don't make public

        /// <summary>
        ///     Assigns domain service extensions to the current instance.
        /// </summary>
        public void SetSevices(DomainServices services)
        {
            this.Services = services;
        }

        public class DomainServices
        {
            readonly Func<Protocol> _getProtocol;

            public DomainServices(Func<Protocol> getProtocol)
            {
                Guard.ArgumentNotNull(getProtocol, nameof(getProtocol));

                _getProtocol = getProtocol;
            }


            /// <summary>
            ///     Gets the protocol being communicated.
            /// </summary>
            public Protocol GetProtocol()
            {
                return _getProtocol();
            }

        }
    }
}