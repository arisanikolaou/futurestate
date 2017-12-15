using FutureState.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     A connection between two devices contained within the same network.
    /// </summary>
    public class DeviceConnection :
        FSEntity,
        IEquatable<DeviceConnection>,
        IEntityMutableKey<Guid>,
        IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     The device that is the source of the connection. Data will move from the source to the target. This can
        ///     be also thought of as the producer id.
        /// </summary>
        public Guid? DeviceSourceId { get; set; }

        /// <summary>
        ///     Gets the address configured on the source interface.
        /// </summary>
        [StringLength(100)]
        public string SourceAddress { get; set; }

        /// <summary>
        ///     The id of the device the information will flow to. This can be thought of as the consumer.
        /// </summary>
        public Guid? DeviceTargetId { get; set; }

        /// <summary>
        ///     Gets the account (stakeholder) to use to connect to the target device.
        /// </summary>
        public Guid? DeviceSourceAccountId { get; set; }

        /// <summary>
        ///     The port configured on the target device or the consuming device.
        /// </summary>
        [StringLength(100)]
        public string TargetAddress { get; set; }

        /// <summary>
        ///     Gets the date in utc the entity was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the date the connection was established.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the date the connection was removed in utc.
        /// </summary>
        public DateTime? DateRemoved { get; set; }

        /// <summary>
        ///     Gets the scenariod the instance is associated with.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the network id.
        /// </summary>
        public Guid? NetworkId { get; set; }

        /// <summary>
        ///     Gets the id of the protocol exchanged between the source and the target devices on the connection.
        /// </summary>
        public Guid? ProtocolId { get; set; }

        /// <summary>
        ///     Gets the device connection's external id.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets/sets the sensitivity level of the data communicated over the interface.
        /// </summary>
        public int? SensitivityLevel { get; set; }

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
        ///     Gets whether the current instance is of equal value to another instance.
        /// </summary>
        public bool Equals(DeviceConnection other)
        {
            if (other == null)
                return false;

            if (other == this)
                return true;

            if (DeviceSourceId == other.DeviceSourceId)
                if (DeviceTargetId == other.DeviceTargetId)
                    return true;

            return false;
        }

        /// <summary>
        ///     Gets/sets the owner of the device model record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the average transactions anticipated per day expressed in total megabytes that is
        ///     sent from the source device to the target/consuming device.
        /// </summary>
        public double? DailyAvgTransactionIo { get; set; }

        /// <summary>
        ///     The average amount of IO persisted on the consuming, target device.
        /// </summary>
        public double? DailyAvgStorageIo { get; set; }

        /// <summary>
        ///     Gets an list of roles/uses that are authorized to access the source device by their role ids.
        /// </summary>
        public List<Guid> TargetRoleAuthorization { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DeviceConnection()
        {
            // public parameterless constructor required by serializers
        }

        /// <summary>
        ///     Creates a new instance with a given name, description and external id.
        /// </summary>
        public DeviceConnection(string displayName, string description = null)
        {
            Id = SeqGuid.Create();

            ExternalId = "";
            DisplayName = displayName ?? "";
            Description = description ?? "";
            DateCreated = DateTime.UtcNow;
            DateAdded = DateCreated;
            DateLastModified = DateCreated;
            UserName = "";
            TargetAddress = "";
        }

        /// <summary>
        ///     Creates a new instance with a given name, description and external id.
        /// </summary>
        public DeviceConnection(string displayName, Device source, Device target, string description = null) : 
            this(displayName, description)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentNotNull(target, nameof(target));

            if (source.ScenarioId != target.ScenarioId)
                throw new ArgumentException("Source device and target device must belong to the same scenario.");

            this.ExternalId = $"{source.ExternalId}-{target.ExternalId}";
            this.DeviceSourceId = source.Id;
            this.DeviceTargetId = target.Id;
            this.ScenarioId = source.Id;
        }

        public class DomainServices 
        {
            readonly Func<Protocol> _getProtocol;
            readonly Func<Device> _getSourceDevice;
            readonly Func<Device> _getTargetDevice;
            readonly Func<IEnumerable<Reference>> _getReferences;

            public DomainServices(
                Func<Protocol> getProtocol, 
                Func<IEnumerable<Reference>> getReferences,
                Func<Device> getSourceDevice,
                Func<Device> getTargetDevice)
            {
                Guard.ArgumentNotNull(getProtocol, nameof(getProtocol));
                Guard.ArgumentNotNull(getReferences, nameof(getReferences));
                Guard.ArgumentNotNull(getSourceDevice, nameof(getSourceDevice));
                Guard.ArgumentNotNull(getTargetDevice, nameof(getTargetDevice));

                this._getProtocol = getProtocol;
                this._getReferences = getReferences;
                this._getSourceDevice = getSourceDevice;
                this._getTargetDevice = getTargetDevice;
            }

            public Protocol GetProtocol() => _getProtocol();

            public IEnumerable<Reference> GetReferences() => _getReferences();

            public Device GetSourceDevice() => _getSourceDevice();

            public Device GetTargetDevice() => _getTargetDevice();
        }
    }
}