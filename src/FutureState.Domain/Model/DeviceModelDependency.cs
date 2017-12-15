using FutureState.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Software required the run the given device. Contains the software model's device configuration.
    /// </summary>
    public class DeviceModelDependency : FSEntity,  IDesignArtefact
    {
        /// <summary>
        ///     The device model the dependency is registered to.
        /// </summary>
        public Guid DeviceModelId { get; set; }

        /// <summary>
        ///     Gets the id of the software model this dependency is based on.
        /// </summary>
        public Guid SoftwareModelDependencyId { get; set; }

        /// <summary>
        ///     Gets the scenario the dependency is registered in.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the external id of the device model.
        /// </summary>
        [StringLength(100)]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the date the entity was created in Utc.
        /// </summary>
        public DateTime DateCreated { get; set; }


        // internal contructor
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DeviceModelDependency()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a device model dependency to a given software model.
        /// </summary>
        /// <param name="softwareModel"></param>
        /// <param name="displayName">The display name of the dependency.</param>
        /// <param name="description">An optional description of the dependency.</param>
        /// <param name="deviceModel"></param>
        public DeviceModelDependency(DeviceModel deviceModel, SoftwareModel softwareModel, string displayName, string description)
        {
            Guard.ArgumentNotNull(deviceModel, nameof(deviceModel));
            Guard.ArgumentNotNull(softwareModel, nameof(softwareModel));

            this.Id = SeqGuid.Create();
            this.ScenarioId = deviceModel.ScenarioId;
            this.DeviceModelId = deviceModel.Id;
            this.SoftwareModelDependencyId = softwareModel.Id;
            this.DisplayName = displayName ?? $"Dependency to '{softwareModel.DisplayName}'";
            this.Description = description ?? "";
            this.DateCreated = DateTime.UtcNow;
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
            readonly Func<SoftwareModel> _getSoftwareModel;
            readonly Func<DeviceModel> _getDeviceModel;

            public DomainServices(Func<SoftwareModel> getSoftwareModel, Func<DeviceModel> getDeviceModel)
            {
                Guard.ArgumentNotNull(getSoftwareModel, nameof(getSoftwareModel));
                Guard.ArgumentNotNull(getDeviceModel, nameof(getDeviceModel));

                _getSoftwareModel = getSoftwareModel;
                _getDeviceModel = getDeviceModel;
            }

            public SoftwareModel GetSoftwareModel() => _getSoftwareModel();

            public DeviceModel GetDeviceModel() => _getDeviceModel();
        }
    }
}