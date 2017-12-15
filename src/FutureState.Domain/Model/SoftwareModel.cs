using FutureState.Data;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Describes a reference model for software.
    /// </summary>
    public class SoftwareModel :
        FSEntity,
        IDesignArtefact,
        IEntityMutableKey<Guid>,
        IFSEntity,
        IAuditable
    {
        /// <summary>
        ///     Gets/sets the owner of the device model record.
        /// </summary>
        [NotEmpty("User name is required.")]
        [StringLength(150)]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        [NotEmptyDate("Date last modified is required.")]
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the design domain this model is governed under.
        /// </summary>
        public Guid? DomainId { get; set; }

        /// <summary>
        ///     Gets the scenariod the instance is associated with.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the software version.
        /// </summary>
        [StringLength(100)]
        public string Version { get; set; }

        /// <summary>
        ///     Gets the software vendor details.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Vendor { get; set; }

        /// <summary>
        ///     Gets the life cycle stage.
        /// </summary>
        [StringLength(200)]
        public string LicenseTypeId { get; set; }

        /// <summary>
        ///     Custom user key pair attributes describing the software model.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Gets the license reference code.
        /// </summary>
        [StringLength(200)]
        public string LicenseReferenceCode { get; set; }

        /// <summary>
        ///     Gets the life cycle stage of the software model.
        /// </summary>
        [NotEmpty("Life cycle id is required.")]
        [StringLength(50)]
        public string LifeCycleId { get; set; }

        /// <summary>
        ///     Gets the date the life cycle was changed.
        /// </summary>
        public DateTime? LifeCycleStageDate { get; set; }

        /// <summary>
        ///     Gets the typical, approximate, fixed code of an installation.
        /// </summary>
        [RangeOrEmpty(0,double.MaxValue)]
        public double? FixedCost { get; set; }

        /// <summary>
        ///     Gets the typical, approximate, annual cost to maintain the service.
        /// </summary>
        [RangeOrEmpty(0, double.MaxValue)]
        public double? AnnualCost { get; set; }

        /// <summary>
        ///     Gets the external id or code for the software model.
        /// </summary>
        [NotEmpty("External id is required.")]
        [StringLength(150)]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SoftwareModel()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SoftwareModel(string displayName, string description, Scenario scenario = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(displayName, nameof(displayName));

            this.Id = SeqGuid.Create();

            if (scenario != null)
                this.ScenarioId = scenario.Id;

            this.ExternalId = displayName;
            this.DisplayName = displayName;
            this.Description = description ?? "";
            this.Version = "";
            this.Vendor = "";
            this.LicenseTypeId = "";
            this.LicenseReferenceCode = "";
            this.LifeCycleId = "Research";
            this.UserName = "";
            this.DateLastModified = DateTime.UtcNow;
            this.Attributes = new List<Item>();
        }

        /// <summary>
        ///     Model service extensions.
        /// </summary>
        public DomainServices Services { get; private set; }
        
        /// <summary>
        ///     Assigns domain service extensions to the current instance.
        /// </summary>
        public void SetSevices(DomainServices services)
        {
            this.Services = services;
        }

        public class DomainServices
        {
            public DomainServices(
                Func<IEnumerable<Reference>> referencesGet,
                Func<IEnumerable<SoftwareModelInterface>> interfacesGet,
                Func<IEnumerable<Capability>> capabilitiesGet,
                Func<IEnumerable<SoftwareModel>> dependenciesGet)
            {
                Guard.ArgumentNotNull(referencesGet, nameof(referencesGet));
                Guard.ArgumentNotNull(interfacesGet, nameof(interfacesGet));
                Guard.ArgumentNotNull(capabilitiesGet, nameof(capabilitiesGet));

                this._referencesGet = referencesGet;
                this._interfacesGet = interfacesGet;
                this._capabilitiesGet = capabilitiesGet;
                this._dependenciesGet = dependenciesGet;
            }

            readonly Func<IEnumerable<Reference>> _referencesGet;
            readonly Func<IEnumerable<SoftwareModelInterface>> _interfacesGet;
            readonly Func<IEnumerable<Capability>> _capabilitiesGet;
            private Func<IEnumerable<SoftwareModel>> _dependenciesGet;

            /// <summary>
            ///     Gets or sets of capabilities that are provided by the model.
            /// </summary>
            public IEnumerable<Capability> GetCapabilities() => _capabilitiesGet();

            /// <summary>
            ///     Gets the dependencies this app needs to run.
            /// </summary>
            public IEnumerable<SoftwareModel> GetDependencies() => _dependenciesGet();

            /// <summary>
            ///     Gets the external interfaces exposed by the softare model e.g. http, tcp etc.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<SoftwareModelInterface> GetInterfaces() => _interfacesGet();

            /// <summary>
            ///     Gets the set of references describing the software model.
            /// </summary>
            public IEnumerable<Reference> GetReferences() => _referencesGet();
        }
    }
}