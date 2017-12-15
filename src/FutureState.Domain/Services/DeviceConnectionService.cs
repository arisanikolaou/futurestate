using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    public class DeviceConnectionService : FsService<DeviceConnection>
    {
        readonly DeviceService _deviceService;
        readonly ProtocolService _protocolService;
        private readonly StakeholderService _stakeholderService;
        private readonly ProjectService _projectService;
        private readonly UserGroupService _userGroupService;

        public DeviceConnectionService(
            FSSecurityContext securityContext,
            ReferenceProvider referenceService,
            StakeholderService stakeholderService,
            UserGroupService userGroupService,
            ProjectService projectService,
            UnitOfWorkLinq<DeviceConnection, Guid> db,
            DeviceService deviceService,
            ProtocolService protocolService,
            IEntityIdProvider<DeviceConnection, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<DeviceConnection> specProvider = null,
            EntityHandler<DeviceConnection, Guid> entityHandler = null)
            : base(securityContext, referenceService, db, keyBinder, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(stakeholderService, nameof(stakeholderService));
            Guard.ArgumentNotNull(userGroupService, nameof(userGroupService));

            _deviceService = deviceService;
            _protocolService = protocolService;
            _projectService = projectService;
            _userGroupService = userGroupService;
            _stakeholderService = stakeholderService;
        }

        /// <summary>
        ///     Defines a new connection between two devices.
        /// </summary>
        public DeviceConnection CreateNew()
        {
            DeviceConnection service = new DeviceConnection();

            Initialize(service);

            return service;
        }

        /// <summary>
        ///     Gets a device connection by its external id.
        /// </summary>
        public DeviceConnection GetByExternalId(string deviceConnectionExternalId) => GetByExternalId(deviceConnectionExternalId, (Guid?)null);

        /// <summary>
        ///     Gets a device connection by its external id and scenario key.
        /// </summary>
        public DeviceConnection GetByExternalId(string deviceConnectionExternalId, Guid? scenarioId) => this.Where(m => m.ExternalId == deviceConnectionExternalId && m.ScenarioId == scenarioId).FirstOrDefault();

        /// <summary>
        ///     Gets a device by its external id as well as the scenario that its associated with.
        /// </summary>
        public DeviceConnection GetByExternalId(string externalId, string scenarioCode)
        {
            if (string.IsNullOrWhiteSpace(externalId))
                return this.Where(m => m.ExternalId == externalId).FirstOrDefault();

            Guid? scenarioCodeId = _projectService.GetScenario(scenarioCode)?.Id;

            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioCodeId).FirstOrDefault();
        }

        /// <summary>
        ///     Calculates the compatible protocols between a source device and a target device by their ids.
        /// </summary>
        public IEnumerable<Protocol> GetCompatibeProtocols(Guid sourceDeviceId, Guid targetDeviceId)
        {
            // TODO: optimize
            Device sourceDevice = _deviceService.GetById(sourceDeviceId);
            Device targetDevice = _deviceService.GetById(targetDeviceId);

            if (sourceDevice == null || targetDevice == null)
                return Enumerable.Empty<Protocol>();

            HashSet<Guid> sourceProtocols = sourceDevice.Services.GetModel().Services.
                GetInterfaces().Where(m => m.ProtocolId.HasValue).Select(m => m.ProtocolId.Value).ToHashSet();

            HashSet<Guid> unionOfProtocols = targetDevice.Services.GetModel().Services
                .GetInterfaces().Where(m => m.ProtocolId.HasValue).Select(m => m.ProtocolId.Value).ToHashSet();

            // union of results
            sourceProtocols.Each(m =>
            {
                if (!unionOfProtocols.Contains(m))
                    unionOfProtocols.Remove(m);
            });

            if (_logger.IsDebugEnabled)
                _logger.Debug("Found {0} compatible protocols", sourceProtocols.Count);

            return _protocolService.GetByIds(unionOfProtocols.ToList());
        }

        /// <summary>
        ///     Gets a protocol by its external id and scenario.
        /// </summary>
        public Protocol GetProtocol(string protocolExternalId, Guid? scenarioId)
        {
            return _protocolService.GetByExternalId(protocolExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets all devices that belong to a given network.
        /// </summary>
        public IEnumerable<Device> GetDevices(Network network) => _deviceService.GetDevices(network);

        /// <summary>
        ///     Gets a device by its device id.
        /// </summary>
        public Device GetDevice(Guid deviceId) => _deviceService.GetById(deviceId);

        /// <summary>
        ///     Initializes the device connection against the service.
        /// </summary>
        public override DeviceConnection Initialize(DeviceConnection deviceConnection)
        {
            // decorate with services
            deviceConnection.SetServices(new DeviceConnection.DomainServices(
                () => deviceConnection.ProtocolId.HasValue ? _protocolService.GetById(deviceConnection.ProtocolId.Value) : null,
                () => GetReferencesProvider().GetReferences(deviceConnection.Id), // gets all references
                () => deviceConnection.DeviceSourceId.HasValue ? GetDevice(deviceConnection.DeviceSourceId.Value) : null, // get source device
                () => deviceConnection.DeviceTargetId.HasValue ? GetDevice(deviceConnection.DeviceTargetId.Value) : null // get target device
                ));

            return deviceConnection;
        }

        public UserGroup GetUserGroupByName(string groupName, Guid? scenarioId)
        {
            return _userGroupService
                .Where(m => m.DisplayName == groupName && m.ScenarioId == scenarioId)
                .FirstOrDefault();
        }

        public void AddUpdate(IEnumerable<DeviceConnection> existingConnections, List<DeviceConnection> connections)
        {
            var splitList = new SplitList<DeviceConnection>();
            splitList.Process(existingConnections, connections, (source, target) => target.Id = source.Id);

            using (Db.Open())
            {
                Add(splitList.New, Db);
                Update(splitList.Existing, Db);

                Db.Commit();
            }
        }
    }
}