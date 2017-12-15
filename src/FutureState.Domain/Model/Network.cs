using FutureState.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FutureState.Specifications;

namespace FutureState.Domain
{
    /// <summary>
    ///     A network is a physical collection of a set of devices, stakeholders and connections.
    /// </summary>
    public class Network : FSEntity, IEntityMutableKey<Guid>,
        IFSEntity, IAsset, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the date the entry was last modified.
        /// </summary>
        [NotEmptyDate("DateLastModified", ErrorMessage = "Date last modified is required.")]
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the name of the user that last modified the entry.
        /// </summary>
        [NotEmpty("UserName", ErrorMessage = "User name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the scenariod the instance is associated with.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the business unit, if any, that actively owns the network.
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <summary>
        ///     Gets the container network to the current instance.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        ///     Gets the external id for the network.
        /// </summary>
        [StringLength(100)]
        [NotEmpty("ExternalId", ErrorMessage = "External Id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     User defined attributes to describe the network.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Gets the type of network to add.
        /// </summary>
        [StringLength(100)]
        public string Type { get; set; }

        /// <summary>
        ///     Gets the fixed cost of the network, e.g. setup costs.
        /// </summary>
        public double? FixedCost { get; set; }

        /// <summary>
        ///     Gets the annualized cost of the network, e.g. maintenance and utilities.
        /// </summary>
        public double? AnnualCost { get; set; }

        /// <summary>
        ///     Gets the date the network was added.
        /// </summary>
        [NotEmptyDate("DateAdded", ErrorMessage = "Date Added is required.")]
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the date the network was wound down.
        /// </summary>
        public DateTime? DateRemoved { get; set; }

        /// <summary>
        ///     Required by serializer. Use other constructor.
        /// </summary>
        public Network()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Network(string externalId, string displayName, string description, Scenario scenario = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(externalId, nameof(externalId));

            Id = SeqGuid.Create();

            DisplayName = displayName ?? "";
            Description = description ?? "";
            ExternalId = externalId ?? "";
            Attributes = new List<Item>();
            Type = "";
            UserName = "";
            DateAdded = DateTime.UtcNow;;
            DateLastModified = DateAdded;

            if (scenario != null)
                ScenarioId = scenario.Id;
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
            readonly Func<IEnumerable<Device>> _devicesGet;
            readonly Func<IEnumerable<Network>> _networksGet;
            readonly Func<IEnumerable<DeviceConnection>> _connectionsGet;
            readonly Func<IEnumerable<Reference>> _referencesGet;
            readonly Func<Scenario> _scenarioGet;
            readonly Func<Network> _containerGet;

            public DomainServices(
                Func<IEnumerable<Device>> devicesGet,
                Func<Network> containerGet,
                Func<IEnumerable<Network>> networksGet,
                Func<Scenario> scenarioGet,
                Func<IEnumerable<DeviceConnection>> connectionsGet,
                Func<IEnumerable<Reference>> referencesGet)
            {
                Guard.ArgumentNotNull(devicesGet, nameof(devicesGet));
                Guard.ArgumentNotNull(networksGet, nameof(networksGet));
                Guard.ArgumentNotNull(connectionsGet, nameof(connectionsGet));
                Guard.ArgumentNotNull(referencesGet, nameof(referencesGet));
                Guard.ArgumentNotNull(containerGet, nameof(containerGet));

                _devicesGet = devicesGet;
                _networksGet = networksGet;
                _connectionsGet = connectionsGet;
                _referencesGet = referencesGet;
                _containerGet = containerGet;
                _scenarioGet = scenarioGet;
            }

            /// <summary>
            ///     The connection between device software installations in the network.
            /// </summary>
            public IEnumerable<DeviceConnection> GetConnections() => _connectionsGet();

            /// <summary>
            ///     Gets the scenario the network belongs to.
            /// </summary>
            public Scenario GetScenario() => _scenarioGet();

            /// <summary>
            ///     A collection of sub-networks contained in the current instance.
            /// </summary>
            public IEnumerable<Network> GetNetworks() => _networksGet();

            /// <summary>
            ///     Gets the containing network.
            /// </summary>
            /// <returns></returns>
            public Network GetContainer() => _containerGet();

            /// <summary>
            ///     Gets all devices that have been deployed on the network.
            /// </summary>
            public IEnumerable<Device> GetDevices() => _devicesGet();

            /// <summary>
            ///     Gets the set of references describing the current instance.
            /// </summary>
            public IEnumerable<Reference> GetReferences() => _referencesGet();
        }

        /// <summary>
        ///     Gets the total cost of the network as well as its devices.
        /// </summary>
        /// <param name="date">
        ///     The reference date to calculate the total cost.
        ///  </param>
        ///  <returns>
        ///     The approximate cost to maintain/setup the network.
        ///  </returns>
        public double GetAnnualCost(DateTime date)
        {
            if (this.Services == null)
                throw new InvalidOperationException("Domain services has not been assigned.");

            if (date < DateAdded)
                return 0;

            if (DateRemoved.HasValue)
                if (DateRemoved.Value < date)
                    return 0;

            var dateSart = this.DateAdded;
            var dateEnd = dateSart.AddYears(1);

            bool initialYear = dateSart <= date && date < dateEnd;

            double totalCost = 0;
            foreach (Device device in Services.GetDevices())
                totalCost += device.GetAnnualCost(date);

            if (FixedCost.HasValue)
                if (initialYear)
                    totalCost += FixedCost.Value;

            if (AnnualCost.HasValue)
                totalCost += AnnualCost.Value;

            // aggregate cost of sub networks.
            foreach (var network in this.Services.GetNetworks())
                totalCost += network.GetAnnualCost(date);

            return totalCost;
        }

        /// <summary>
        ///     Gets the total benefit provide by the network.
        /// </summary>
        /// <param name="date">The reference date to calculate the total benefit.</param>
        /// <returns>The approximate total benefit of the network and all contained networks.</returns>
        public double GetAnnualBenefit(DateTime date)
        {
            if (Services == null)
                throw new InvalidOperationException("Domain services has not been assigned.");

            if (date < DateAdded)
                return 0;

            if (DateRemoved.HasValue)
                if (DateRemoved.Value < date)
                    return 0;

            var dateSart = DateAdded;
            var dateEnd = dateSart.AddYears(1);

            bool initialYear = dateSart <= date && date < dateEnd;

            double totalBenefit = 0;
            var devices = Services.GetDevices();
            foreach (Device device in devices)
                totalBenefit += device.GetAnnualBenefit(date);

            // aggregate cost of sub networks.
            var networks = this.Services.GetNetworks();
            foreach (var network in networks)
                totalBenefit += network.GetAnnualBenefit(date);

            return totalBenefit;
        }
    }
}