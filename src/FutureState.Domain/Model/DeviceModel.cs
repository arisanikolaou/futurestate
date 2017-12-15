using FutureState.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Describes a reference architecture for a device.
    /// </summary>
    public class DeviceModel : FSEntity, IEntityMutableKey<Guid>,  IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the scenario that the device model is running in.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the external id of the device model.
        /// </summary>
        [StringLength(100)]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the date the entry was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the governing domain id for the device.
        /// </summary>
        public Guid? DomainId { get; set; }

        /// <summary>
        ///     Gets the fixed cost to maintain the asset.
        /// </summary>
        public double? FixedCost { get; set; }

        /// <summary>
        ///     Gets/sets the annual approximate cost to maintain the asset.
        /// </summary>
        public double? AnnualCost { get; set; }

        /// <summary>
        ///     User defined attributes to describe the device model.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Gets the state of the model.
        /// </summary>
        [StringLength(100)]
        public string LifeCycleId { get; set; }

        /// <summary>
        ///     Gets the date the life-cyle characterization was made.
        /// </summary>
        public DateTime? LifeCycleStageDate { get; set; }

        /// <summary>
        ///     Gets the version of the device.
        /// </summary>
        [StringLength(100)]
        public string ModelVersion { get; set; }

        /// <summary>
        ///     Gets/sets the owner of the device model record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }
        
        // internal .ctor for serializers
        public DeviceModel()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance with a  given display name and description.
        /// </summary>
        public DeviceModel(string externalId, string displayName, string description = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(externalId,nameof(externalId));
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(displayName, nameof(displayName));

            // assign new id
            this.Id = SeqGuid.Create();

            this.DisplayName = displayName;
            this.Description = description ?? "";
            this.ModelVersion = "";
            this.LifeCycleId = "Research";
            this.ExternalId = externalId ?? "";
            this.DateCreated = DateTime.UtcNow;
            this.DateLastModified = this.DateCreated;
            this.LifeCycleStageDate = this.DateCreated;
            this.Attributes = new List<Item>();
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


        public class DomainServices 
        {
            public DomainServices(
                Func<IEnumerable<SoftwareModelInterface>> interfacesGet,
                Func<IEnumerable<DeviceModelDependency>> softwareModelGet,
                Func<IEnumerable<Capability>> capabilitiesGet,
                Func<IEnumerable<Reference>> referencesGet,
                Func<DesignDomain> domainGet,
                Func<Scenario> scenarioGet)
            {
                this._interfacesGet = interfacesGet ?? throw new ArgumentNullException(nameof(interfacesGet));
                this._softwareModelGet = softwareModelGet ?? throw new ArgumentNullException(nameof(softwareModelGet));
                this._capabilitiesGet = capabilitiesGet ?? throw new ArgumentNullException(nameof(capabilitiesGet));
                this._referencesGet = referencesGet ?? throw new ArgumentNullException(nameof(referencesGet));
                this._domainGet = domainGet ?? throw new ArgumentNullException(nameof(domainGet));
                this._scenarioGet = scenarioGet ?? throw new ArgumentNullException(nameof(scenarioGet));
            }

            readonly Func<IEnumerable<SoftwareModelInterface>> _interfacesGet;
            readonly Func<IEnumerable<DeviceModelDependency>> _softwareModelGet;
            readonly Func<IEnumerable<Capability>> _capabilitiesGet;
            readonly Func<IEnumerable<Reference>> _referencesGet;
            readonly Func<DesignDomain> _domainGet;
            readonly Func<Scenario> _scenarioGet;

            /// <summary>
            ///     Gets the software ports that have been registered on the device.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<SoftwareModelInterface> GetInterfaces() => _interfacesGet();

            /// <summary>
            ///     Gets the design domain governing the device model.
            /// </summary>
            /// <returns></returns>
            public DesignDomain GetDomain() => _domainGet();

            /// <summary>
            ///     Gets the software installations made on the local machine.
            /// </summary>
            public IEnumerable<DeviceModelDependency> GetDependencies() => _softwareModelGet();

            /// <summary>
            ///     Gets the set of capabilities provided by the device.
            /// </summary>
            public IEnumerable<Capability> GetCapabilities() => _capabilitiesGet();

            /// <summary>
            ///     Gets the set of references describing the current instance.
            /// </summary>
            public IEnumerable<Reference> GetReferences() => _referencesGet();

            /// <summary>
            ///     Gets the scenari the device model belongs to.
            /// </summary>
            public Scenario GetScenario() => _scenarioGet();
        }
    }
}