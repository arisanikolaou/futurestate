using FutureState.Data.Keys;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    public class DeviceService : FsService<Device>, IService
    {
        readonly FsUnitOfWork _sfDb;
        private readonly DeviceModelService _deviceModelService;
        private readonly ProjectService _projectService;

        public DeviceService(
            FSSecurityContext securityContext,
            ProjectService projectService,
            ReferenceProvider referenceService,
            FsUnitOfWork db,
            DeviceModelService deviceModelService,
            IEntityIdProvider<Device, Guid> idProvider)
            : base(securityContext, referenceService, db, idProvider, deviceModelService?.MessagePipe)
        {
            _sfDb = db; // null reference checked by base class
            _deviceModelService = deviceModelService;
            _projectService = projectService;
        }

        public Device GetByExternalId(string externalId)
        {
            return GetByExternalId(externalId, null);
        }

        public Device GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        public DeviceModel GetDeviceModel(Device device)
        {
            Guard.ArgumentNotNull(device, nameof(device));

            if (device.DeviceModelId.HasValue)
                return _deviceModelService.GetById(device.DeviceModelId.Value);

            return null;
        }

        protected override void OnBeforeDelete(Guid key)
        {
            // delete dependencies
            GetReferencesProvider().RemoveReferences(key, _sfDb);

            // before delete
            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Initializes a device model to the service,
        /// </summary>
        public override Device Initialize(Device entity)
        {
            // attach services to the device model  context
            entity.SetServices( new Device.DomainServices(
                () => GetReferencesProvider().GetReferences(entity.Id),
                () => GetDeviceModel(entity),
                () => entity.ScenarioId.HasValue ? _projectService.GetScenarioById(entity.ScenarioId.Value) : null // resolve scenario
            ));

            return base.Initialize(entity);
        }

        public IEnumerable<Device> GetDevices(Network network)
        {
            Guard.ArgumentNotNull(network, nameof(network));

            return Where(m => m.NetworkId == network.Id);
        }
    }
}