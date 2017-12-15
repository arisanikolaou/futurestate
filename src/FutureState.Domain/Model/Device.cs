using FutureState.Data;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     A concrete asset that belongs to a given network. Devices are based on a given <see cref="DeviceModel"/>.
    /// </summary>
    public class Device : FSEntity, IEntityMutableKey<Guid>, IAsset, IDesignArtefact
    {
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
        ///     Gets the scenario id associated with the current instance.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the key of the device model the current instance is based on.
        /// </summary>
        [NotEmpty("Device Model Id is required.")]
        public Guid? DeviceModelId { get; set; }

        /// <summary>
        ///     Gets the network the current instance has been deployed to.
        /// </summary>
        public Guid NetworkId { get; set; }

        /// <summary>
        ///     Gets the end date of a given device.
        /// </summary>
        public DateTime? DateRemoved { get; set; }

        /// <summary>
        ///     Gets the device's availability tier.
        /// </summary>
        public string AvailabilityTier { get; set; }

        /// <summary>
        ///     Gets the date the device was deployed.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the asset's external identifier.
        /// </summary>
        [StringLength(100)]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the annual cost to maintain the device or null if not known. This is the hardware cost.
        /// </summary>
        public double? AnnualCost { get; set; }

        /// <summary>
        ///     Gets the fixed cost required to maintain the device or null if not known. This is the hardware cost.
        /// </summary>
        public double? FixedCost { get; set; }

        /// <summary>
        ///     User defined attributes to describe the network.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Required by serializers. Use other constructors.
        /// </summary>
        public Device()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance with a given name, description and external id.
        /// </summary>
        public Device(Network network, string name, string description, string externalId)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            this.Id = SeqGuid.Create();

            this.DisplayName = name ?? "";
            this.ScenarioId = network.ScenarioId;
            this.NetworkId = network.Id;
            this.Description = description ?? "";
            this.DateAdded = DateTime.UtcNow;
            this.AvailabilityTier = ""; // bronze etc
            this.ExternalId = externalId ?? "";
            this.Attributes = new List<Item>();
        }

        // generic domain context appends behaviour to the domain object
        public class DomainServices
        {
            private readonly Func<IList<Reference>> _referencesGet;
            private readonly Func<DeviceModel> _deviceModelsGet;
            private readonly Func<Scenario> _scenarioGet;

            public DomainServices(
                Func<IList<Reference>> referencesGet,
                Func<DeviceModel> devicesModelsGet,
                Func<Scenario> scenarioGet)
            {
                Guard.ArgumentNotNull(referencesGet, nameof(referencesGet));
                Guard.ArgumentNotNull(devicesModelsGet, nameof(devicesModelsGet));
                Guard.ArgumentNotNull(devicesModelsGet, nameof(devicesModelsGet));

                this._referencesGet = referencesGet;
                this._deviceModelsGet = devicesModelsGet;
                this._scenarioGet = scenarioGet;
            }

            /// <summary>
            ///     Gets the scenario, if any, the device belongs to.
            /// </summary>
            /// <returns></returns>
            public Scenario GetScenario() => _scenarioGet();

            /// <summary>
            ///     Gets the set of references describing the current instance.
            /// </summary>
            public IList<Reference> GetReferences() => _referencesGet();

            /// <summary>
            ///     Gets the device model the implementation is based on.
            /// </summary>
            public DeviceModel GetModel() => _deviceModelsGet();
        }

        /// <summary>
        ///     Gets the approximate total cost of owning a given instance of this software as of a given date.
        /// </summary>
        /// <param name="date">The reference date to calculate the devices total cost in Utc.</param>
        public double GetAnnualCost(DateTime date)
        {
            if (this.Services == null)
                throw new InvalidOperationException();

            if (date < DateAdded)
                return 0;

            if (DateRemoved.HasValue)
                if (DateRemoved.Value < date)
                    return 0;

            var model = this.Services.GetModel();
            var dateSart = this.DateAdded;
            var dateEnd = this.DateAdded.AddYears(1);

            if (model.Services == null)
                throw new InvalidOperationException();

            bool initialYear = dateSart <= date && date < dateEnd;

            var dependencies = model.Services.GetDependencies();
            // |Year 0|Year 1
            // |fixed cost and annaul cost | annual cost

            double totalCost = 0;
            foreach (DeviceModelDependency dependency in dependencies)
            {
                if (dependency.Services == null)
                    throw new InvalidOperationException();

                SoftwareModel softwareModel = dependency.Services.GetSoftwareModel();

                // accumulate fixed cost year 1
                if (initialYear)
                    totalCost += softwareModel.FixedCost.HasValue ? softwareModel.FixedCost.Value : 0;

                // annual license cost
                totalCost += softwareModel.AnnualCost.HasValue ? softwareModel.AnnualCost.Value : 0;
            }

            // add device cost
            if (initialYear)
                totalCost += FixedCost.HasValue ? FixedCost.Value : 0;

            totalCost += AnnualCost.HasValue ? AnnualCost.Value : 0;

            return totalCost;
        }

        /// <summary>
        ///     Gets the value of all benefits provided by the device.
        /// </summary>
        /// <param name="date">The reference date to calculate the devices's total benefit.</param>
        public double GetAnnualBenefit(DateTime date)
        {
            if (this.Services == null)
                throw new InvalidOperationException();

            if (date < DateAdded)
                return 0;

            if (DateRemoved.HasValue)
                if (DateRemoved.Value < date)
                    return 0;

            var model = this.Services.GetModel();
            var dateSart = this.DateAdded;
            var dateEnd = this.DateAdded.AddYears(1);

            if (model.Services == null)
                throw new InvalidOperationException();

            bool initialYear = dateSart <= date && date < dateEnd;

            IEnumerable<DeviceModelDependency> dependencies = model.Services.GetDependencies();
            // |Year 0|Year 1
            // |fixed cost and annaul cost | annual cost

            double totalBenefit = 0;
            var accountedFor = new HashSet<Guid>();

            foreach (DeviceModelDependency dependency in dependencies)
            {
                if (dependency.Services == null)
                    throw new InvalidOperationException();

                SoftwareModel sm = dependency.Services.GetSoftwareModel();

                totalBenefit = CalculateTotalBenefit(initialYear, sm, accountedFor);
            }

            return totalBenefit;
        }

        double CalculateTotalBenefit(bool initialYear, SoftwareModel sm, HashSet<Guid> accountedFor)
        {
            if (accountedFor.Contains(sm.Id))
            {
                 // already accounted for
                return 0;
            }

            // log account
            accountedFor.Add(sm.Id);

            double totalBenefit = 0;

            foreach (var capability in sm.Services.GetCapabilities())
            {
                // accumulate fixed cost year 1
                if (initialYear)
                    if (capability.BusinessValue.HasValue)
                        totalBenefit += capability.BusinessValue.Value;

                if (capability.AnnualBusinessValue.HasValue)
                    totalBenefit += capability.AnnualBusinessValue.Value;
            }

            // calculate benefits of all dependencies
            foreach (var dependency in sm.Services.GetDependencies())
                totalBenefit += CalculateTotalBenefit(initialYear, sm, accountedFor);

            return totalBenefit;
        }
    }
}