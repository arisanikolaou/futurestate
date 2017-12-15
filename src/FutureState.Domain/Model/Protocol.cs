using FutureState.Data;
using System;
using System.ComponentModel.DataAnnotations;
using FutureState.Specifications;

namespace FutureState.Domain
{
    /// <summary>
    ///     Protocols define contracts of communication between two different end points 
    ///     on an interface.
    /// </summary>
    /// <remarks>
    ///     Protocols can be packaged and composed in a stack e.g. xml document with schema xyz is packaged
    ///     in a tcp protocol container.
    /// </remarks>
    public class Protocol : FSEntity, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the user that last modified the entry.
        /// </summary>
        [NotEmpty("UserName", ErrorMessage = "User name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified in utc.
        /// </summary>
        [NotEmptyDate("DateLastModified", ErrorMessage = "Date last modified is required.")]
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the protocol version.
        /// </summary>
        [StringLength(100)]
        public string Version { get; set; }

        /// <summary>
        ///     Gets the external id of the component.
        /// </summary>
        [StringLength(100)]
        [NotEmpty("ExternalId",ErrorMessage = "External id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the container/parent identifierr.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        ///     Gets/sets the scenario the protocol is associated with.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the protocol stereotype.
        /// </summary>
        [StringLength(100)]
        public string ProtocolTypeId { get; set; }


        // requried for serializer
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Protocol()
        {
            // required by the serializer
        }

        /// <summary>
        ///     Creates a new instance with a given name and a description.
        /// </summary>
        public Protocol(string externalId, string displayName, string description = "", Protocol parent = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(externalId, nameof(externalId));

            Id = SeqGuid.Create();

            DisplayName = displayName ?? "";
            Description = description ?? "";
            ExternalId = externalId ?? "";
            ProtocolTypeId = "Other";
            DateLastModified = DateTime.UtcNow;
            UserName = "";
            Version = "";

            if (parent != null)
            {
                ParentId = parent.Id;
                ScenarioId = parent.ScenarioId;
            }
        }

        /// <summary>
        ///     Model service extensions.
        /// </summary>
        public DomainServices Services { get; private set; }

        /// <summary>
        ///     Assigns the implementation of <see cref="DomainServices"/>.
        /// </summary>
        /// <param name="service">The service implementation.</param>
        public void SetServices(DomainServices service) => Services = service;

        /// <summary>
        ///     Services to help navigate to related objects.
        /// </summary>
        public class DomainServices
        {
            private readonly Func<Protocol> _getProtocol;

            public DomainServices(Func<Protocol> protocolGet)
            {
                Guard.ArgumentNotNull(protocolGet, nameof(protocolGet));

                _getProtocol = protocolGet;
            }

            /// <summary>
            ///     Gets the containing protocol.
            /// </summary>
            /// <returns></returns>
            public Protocol GetContainer()
            {
                return _getProtocol();
            }
        }
    }
}