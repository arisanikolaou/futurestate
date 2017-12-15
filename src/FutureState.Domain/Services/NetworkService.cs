using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Services;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Service used to add, remove and update networks.
    /// </summary>
    public class NetworkService : FsService<Network>, IService
    {
        private readonly DeviceService _deviceService;
        private readonly FsUnitOfWork _db;
        private readonly StakeholderService _stakeholderService;
        private readonly DeviceConnectionService _deviceConnectionService;
        private readonly ProtocolService _protocolService;
        private readonly DeviceModelService _deviceModelService;
        private readonly ProjectService _projectService;
        private readonly BusinessUnitProvider _businessUnitService;
        private readonly ApplicationAccessService _appAccessService;

        /// <summary>
        ///     Creates a new instance given a set of primitive services.
        /// </summary>
        public NetworkService(
            FSSecurityContext securityContext,
            ReferenceProvider referenceProvider,
            BusinessUnitProvider businessUnitProvider,
            StakeholderService stakeholdersService,
            ProtocolService protocolService,
            DeviceModelService deviceModelService,
            DeviceService deviceService,
            DeviceConnectionService deviceConnectionService,
            ApplicationAccessService appAccessService,
            ProjectService projectService,
            FsUnitOfWork db,
            IEntityIdProvider<Network, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<Network> specProvider = null,
            EntityHandler<Network, Guid> entityHandler = null) : base(securityContext, referenceProvider, db, idProvider, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(stakeholdersService, nameof(stakeholdersService));
            Guard.ArgumentNotNull(protocolService, nameof(protocolService));
            Guard.ArgumentNotNull(deviceService, nameof(deviceService));
            Guard.ArgumentNotNull(deviceConnectionService, nameof(deviceConnectionService));
            Guard.ArgumentNotNull(deviceModelService, nameof(deviceModelService));
            Guard.ArgumentNotNull(projectService, nameof(projectService));
            Guard.ArgumentNotNull(businessUnitProvider, nameof(businessUnitProvider));
            Guard.ArgumentNotNull(appAccessService, nameof(appAccessService));

            // compose device service
            // todo: dependency inject

            //setup and configure service
            _deviceService = deviceService;
            _appAccessService = appAccessService;
            _businessUnitService = businessUnitProvider;
            _projectService = projectService;
            _deviceModelService = deviceModelService;
            _protocolService = protocolService;
            _deviceConnectionService = deviceConnectionService;
            _stakeholderService = stakeholdersService;

            _db = db;
        }

        /// <summary>
        ///     Gets any error in the state of given device.
        /// </summary>
        public IEnumerable<Error> Validate(Device device)
        {
            return _deviceService.Validate(device);
        }

        /// <summary>
        ///     Gets a business unit by its external key.
        /// </summary>
        public BusinessUnit GetBusinessUnit(string businessUnitExternalId)
        {
            return _businessUnitService.GetByExternalId(businessUnitExternalId);
        }

        /// <summary>
        ///     Gets a device by its external key that belongs to a given scenario.
        /// </summary>
        public Device GetDevice(string deviceModelExternalId, Guid? scenarioId)
        {
            return _deviceService.GetByExternalId(deviceModelExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets a device connection by its external key.
        /// </summary>
        public DeviceConnection GetDeviceConnection(string deviceConnectionExternalId, Guid? scenarioId)
        {
            return _deviceConnectionService.GetByExternalId(deviceConnectionExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetScenario(string scenarioExternalId)
        {
            return _projectService.GetScenario(scenarioExternalId);
        }

        /// <summary>
        ///     Gets a network by its external key belonging to a given scenario.
        /// </summary>
        public Network GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a network by its external id belonging to the default scenario.
        /// </summary>
        public Network GetByExternalId(string externalId)
        {
            return GetByExternalId(externalId, null); 
        }

        /// <summary>
        ///     Gets a stakeholder by its external key and to a given scenario.
        /// </summary>
        public Stakeholder GetStakeholder(string stakeholderExternalId, Guid? scenarioId)
        {
           return  _stakeholderService.GetByExternalId(stakeholderExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets the device model a given device is based on.
        /// </summary>
        public DeviceModel GetDeviceModel(Device device)
        {
            Guard.ArgumentNotNull(device, nameof(device));

            if (device.DeviceModelId.HasValue)
                return _deviceModelService.GetById(device.DeviceModelId.Value);
            else
                return null;
        }

        /// <summary>
        ///     Gets a device model by its external id as well as a scenario id.
        /// </summary>
        /// <returns></returns>
        public DeviceModel GetDeviceModel(string deviceModelExternalId, Guid? scenarioId)
        {
            return _deviceModelService.GetByExternalId(deviceModelExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets a protocol by its external id and by a given scenario.
        /// </summary>
        public Protocol GetProtocol(string protocolKey, Guid? scenarioId)
        {
            return _protocolService.GetByExternalId(protocolKey, scenarioId);
        }

        /// <summary>
        ///     Registers a new connection a given network. The connection and information flow is assumed to be unidirectional.
        /// </summary>
        public void AddConnections(Network network, IEnumerable<DeviceConnection> connections)
        {
            Guard.ArgumentNotNull(network, nameof(network));
            Guard.ArgumentNotNull(connections, nameof(connections));

            connections.Each(connection =>
            {
                connection.NetworkId = network.Id;
                connection.ScenarioId = network.ScenarioId;
            });

            _deviceConnectionService.Add(connections);
        }

        /// <summary>
        ///     Gets all connections between devices on a given network.
        /// </summary>
        public IList<DeviceConnection> GetConnections(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            return _deviceConnectionService.Where(m => m.NetworkId == network.Id).ToList();
        }

        /// <summary>
        ///     Gets all devices registered on a given network.
        /// </summary>
        /// <param name="network">Required. The network the query.</param>
        /// <returns></returns>
        public IList<Device> GetDevices(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            return _deviceService.Where(m => m.NetworkId == network.Id).ToList();
        }

        /// <summary>
        ///     Attaches the entity to the service context;
        /// </summary>
        public override Network Initialize(Network entity)
        {

            var services = new Network.DomainServices(
                () => GetDevices(entity),
                () => GetContainingNetwork(entity),
                () => GetSubNetworks(entity),
                () => entity.ScenarioId.HasValue ? _projectService.GetScenarioById(entity.ScenarioId.Value) : null, // get scenario
                () => GetConnections(entity),
                () => GetReferencesProvider().GetReferences(entity.Id)
            );

            entity.SetServices(services);

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets the networks that are contained in a given network.
        /// </summary>
        public IEnumerable<Network> GetSubNetworks(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            return base.Where(m => m.ParentId == network.Id);
        }

        /// <summary>
        ///     Gets the network that contains a given network instance.
        /// </summary>
        public Network GetContainingNetwork(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            if (network.ParentId == null)
                return null;

            return Where(m => m.Id == network.ParentId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the network that contains a given network instance.
        /// </summary>
        public Network GetContainingNetwork(Network network, FsUnitOfWork db)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            if (network.ParentId == null)
                return null;

            return Where(m => m.Id == network.ParentId, db).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        /// <param name="connections"></param>
        public void AddUpdateConnections(Network network, List<DeviceConnection> connections)
        {
            Guard.ArgumentNotNull(network, nameof(network));
            Guard.ArgumentNotNull(connections, nameof(connections));

            if (!connections.Any())
                return;

            connections.Each(connection =>
            {
                connection.NetworkId = network.Id;
                connection.ScenarioId = network.ScenarioId;
            });

            var existingConnections =
                _deviceConnectionService.Where(m => m.ScenarioId == network.ScenarioId && m.Id == network.Id);

            _deviceConnectionService.AddUpdate(existingConnections, connections);
        }

        /// <summary>
        ///     Registers a new device on a given network.
        /// </summary>
        public void AddDevices(Network network, IEnumerable<Device> devices)
        {
            Guard.ArgumentNotNull(network, nameof(network));
            Guard.ArgumentNotNull(devices, nameof(devices));

            // fixup scenario id
            devices.Each(device =>
            {
                device.ScenarioId = network.ScenarioId;
                device.NetworkId = network.Id;
            });

            // save
            _deviceService.Add(devices);
        }

        /// <summary>
        ///     Gets all the devices that a given stakeholder can access.
        /// </summary>
        public IEnumerable<Device> GetDevicesAccessibleTo(
            Network network,
            Stakeholder stakeholder,
            Scenario scenario)
        {
            using (_db.Open())
            {
                // find by stakeholder assignment
                var appAccessByLoginIds = GetAppsAccessibleTo(network, stakeholder, scenario, _db);

                // list of device ids that the user can access
                var deviceIds = appAccessByLoginIds.Select(m => m.DeviceId).ToList();

                // gets set of devices the user can access
                var devices = this._db.Devices.LinqReader
                    .Where(m => m.NetworkId == network.Id && scenario.Id == m.ScenarioId)
                    .ToList()
                    .Where(m => deviceIds.Contains(m.Id));

                return devices.ToList();
            }
        }

        /// <summary>
        ///     Gets the device connections that a given user can access.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public IList<DeviceConnection> GetConnectionsAccessibleTo(
            IEnumerable<Device> devices, 
            Stakeholder stakeholder,
            Scenario scenario)
        {
            var allConnections = new List<DeviceConnection>();

            using (_db.Open())
            {
                // get login
                var logins = _db.StakeholderLogins
                    .LinqReader.Where(m => m.StakeholderId == stakeholder.Id 
                    && m.ScenarioId == scenario.Id);

                // get memberships
                var groups = _db.UserGroupMemberships.LinqReader
                    .Where(m => m.MemberId == stakeholder.Id && m.ScenarioId == scenario.Id);
                
                // get list of group ids
                var identities = groups
                    .Select(m => m.Id)
                    .Combine(logins.Select(m => m.Id))
                    .ToList();

                foreach (Device device in devices)
                {
                    IList<DeviceConnection> targetConnectionDevices = _deviceConnectionService
                        .Where(m =>  m.DeviceTargetId == device.Id, _db)
                        .ToList();

                    // filter connections
                    foreach(DeviceConnection connection in targetConnectionDevices)
                    {
                        // the infomration provider
                        if (connection.TargetRoleAuthorization != null 
                            && connection.TargetRoleAuthorization.Any())
                        {
                            // determine if role is authorized
                            if(connection.TargetRoleAuthorization
                                .Any(m => identities.Contains(m))){
                                // authorized
                                allConnections.Add(connection);
                            }
                        }
                        else
                        {
                            // assume all users have access
                            allConnections.Add(connection);
                        }
                    }
                }
            }

            return allConnections;
        }
        
        /// <summary>
        ///     Gets all the devices that a given stakeholder can access.
        /// </summary>
        public IEnumerable<ApplicationAccess> GetAppsAccessibleTo(
            Network network,
            Stakeholder stakeholder,
            Scenario scenario, FsUnitOfWork db)
        {
            // get loginn
            var logins = db.StakeholderLogins
                .LinqReader.Where(m => m.StakeholderId == stakeholder.Id && m.ScenarioId == scenario.Id);
            var loginIds = logins.Select(m => m.Id);

            // get memberships
            var groups = db.UserGroupMemberships.LinqReader
                .Where(m => m.MemberId == stakeholder.Id && m.ScenarioId == scenario.Id);
            var groupIds = groups.Select(m => m.GroupId);

            // find by user group membership
            var appAccessByGroupIds = _db.ApplicationAccess
                .LinqReader.Where(m => m.UserGroupId != null && m.ScenarioId == scenario.Id)
                .ToList()
                .Where(m => groupIds.Contains(m.UserGroupId.Value));

            // find by stakeholder assignment
            List<ApplicationAccess> appAccess = this._db.ApplicationAccess
                .LinqReader.Where(m => m.StakeholderLoginId != null && m.ScenarioId == scenario.Id)
                .ToList() // contains can't be processed by underlying provider
                .Where(m => loginIds.Contains(m.StakeholderLoginId.Value))
                .ToList();

            return appAccess;
        }

        /// <summary>
        ///     Adds and optionally updates a set of devices.
        /// </summary>
        public void AddUpdateDevices(IEnumerable<Device> devicesToAdd, IEnumerable<Device> devicesToUpdate)
        {
            _deviceService.AddUpdate(devicesToAdd, devicesToUpdate);
        }

        protected override void OnBeforeDelete(Guid key)
        {
            // session is open

            // delete connections first, then devices
            _deviceConnectionService.Remove(m => m.NetworkId == key, _db);
            _deviceService.Remove(m => m.NetworkId == key, _db);

            GetReferencesProvider().RemoveReferences(key, _db);

            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Removes device connections associated with a given network.
        /// </summary>
        public void RemoveConnections(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            _deviceConnectionService.Remove(m => m.NetworkId == network.Id, _db);
        }

        /// <summary>
        ///     Removes devices associated with a given network.
        /// </summary>
        public void RemoveDevices(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            _deviceService.Remove(m => m.NetworkId == network.Id);
        }

        protected override void OnBeforeAdd(Network entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);

            DemandLineageIntegrity(entity);
        }

        protected override void OnBeforeUpdate(Network entity)
        {
            base.OnBeforeUpdate(entity);

            DemandLineageIntegrity(entity);
        }

        /// <summary>
        ///     Ensures that a given network does not exist in its own network hierarchy.
        /// </summary>
        protected void DemandLineageIntegrity(Network entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            if (entity.ParentId == null)
                return;

            // should never happend as it will be caught by a simple validation rule
            if (entity.ParentId == entity.Id)
            {
                string msg = $"The network {entity.DisplayName} already exists in the instance's own lineage.";
                throw new RuleException(msg);
            }

            // assume that session is open and validate its lineage
            // already checked parent, start with parent and iterate recursively
            var container = GetById(entity.ParentId.Value, _db);
            while (container != null)
            {
                if (container.Id == entity.Id)
                {
                    string msg = $"The network {entity.DisplayName} already exists in the instance's own lineage.";
                    throw new RuleException(msg);
                }

                if(container.ScenarioId != entity.ScenarioId)
                {
                    string msg = $"Can't update network {entity.DisplayName} as the network belongs to a different scenario from its container {container.DisplayName}.";
                    throw new RuleException(msg);
                }

                container = GetContainingNetwork(container, _db);
            }
        }
    }
}