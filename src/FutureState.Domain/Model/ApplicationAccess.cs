using FutureState.Data;
using FutureState.Specifications;
using System;

namespace FutureState.Domain
{
    /// <summary>
    ///     A record indicating which user groups or stakeholders can access 
    ///     a given application running on a device.
    /// </summary>
    public class ApplicationAccess : FSEntity, IEntityMutableKey<Guid>, IDesignArtefact
    {
        /// <summary>
        ///     Gets the user group that can access the given application.
        /// </summary>
        public Guid? UserGroupId { get; set; }

        /// <summary>
        ///     Gets the login that can access the application.
        /// </summary>
        [NotEmptyGuid("Stakeholder login id must be supplied.")]
        public Guid? StakeholderLoginId { get; set; }

        /// <summary>
        ///     Gets/sets the external key to the entry.
        /// </summary>
        [NotEmpty("External Id is requried.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets/sets the application the user or group can access.
        /// </summary>
        [NotEmptyGuid("Device model dependency id must be supplied.")]
        public Guid? DeviceModelDependencyId { get; set; }

        /// <summary>
        ///     Gets the device instance that the user or group can access.
        /// </summary>
        [NotEmptyGuid("Device model dependency id must be supplied.")]
        public Guid? DeviceId { get; set; }

        /// <summary>
        ///     Gets the user that last modified the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the scenario the entity belongs to.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ApplicationAccess()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="device">Gets the device the login or group has access to.</param>
        /// <param name="deviceModelDependency">Gets the application/device model the login has access to.</param>
        /// <param name="login"></param>
        public ApplicationAccess(Device device, DeviceModelDependency deviceModelDependency, StakeholderLogin login)
        {
            Guard.ArgumentNotNull(device, nameof(device));
            Guard.ArgumentNotNull(deviceModelDependency, nameof(deviceModelDependency));
            Guard.ArgumentNotNull(login, nameof(login));

            this.DeviceId = device.Id;
            this.DeviceModelDependencyId = deviceModelDependency.Id;
            this.StakeholderLoginId = login.Id;
            this.DateLastModified = DateTime.UtcNow;
            this.UserName = "";
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="device">Gets the device the login or group has access to.</param>
        /// <param name="deviceModelDependency">Gets the application/device model the login has access to.</param>
        /// <param name="group"></param>
        public ApplicationAccess(Device device, DeviceModelDependency deviceModelDependency, UserGroup group)
        {
            Guard.ArgumentNotNull(device, nameof(device));
            Guard.ArgumentNotNull(deviceModelDependency, nameof(deviceModelDependency));
            Guard.ArgumentNotNull(group, nameof(group));

            this.DeviceId = device.Id;
            this.DeviceModelDependencyId = deviceModelDependency.Id;
            this.UserGroupId = group.Id;
            this.DateLastModified = DateTime.UtcNow;
            this.UserName = "";

            this.Id = SeqGuid.Create();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ApplicationAccess(StakeholderLogin login)
        {
            Guard.ArgumentNotNull(login, nameof(login));

            this.DateLastModified = DateTime.UtcNow;
            this.UserName = "";
            this.StakeholderLoginId = login.Id;
            this.Id = SeqGuid.Create();
        }
    }
}
