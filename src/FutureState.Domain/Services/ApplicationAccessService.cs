using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Specifications;
using System;
using System.Linq;

namespace FutureState.Domain.Services
{
    public class ApplicationAccessService : ProviderLinq<ApplicationAccess, Guid>
    {
        private readonly DeviceService _deviceService;
        private readonly FsUnitOfWork _db;

        public ApplicationAccessService(
            FsUnitOfWork db,
            DeviceService deviceService,
            IEntityIdProvider<ApplicationAccess, Guid> keyBinder,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<ApplicationAccess> specProvider = null,
            EntityHandler<ApplicationAccess,Guid> entityHandler = null) 
            : base(db, keyBinder, messagePipe, specProvider, entityHandler)
        {
            _deviceService = deviceService;
            _db = db;
        }

        /// <summary>
        ///     Gets an application access record by its external key as well as scenario id.
        /// </summary>
        /// <param name="externalId">The external id to search for.</param>
        /// <param name="scenarioId">Gets the scenario the access belongs to.</param>
        /// <returns></returns>
        public ApplicationAccess GetByExternalId(string externalId, Guid? scenarioId) => Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();

        /// <summary>
        ///     Gets a devices by its external id and scenario.
        /// </summary>
        public Device GetDevice(string deviceExternalId, Scenario scenario) => this._deviceService.GetByExternalId(deviceExternalId, scenario?.Id);

        /// <summary>
        ///     Gets a software model record by external id and scenario.
        /// </summary>
        public SoftwareModel GetSoftwareModel(string softwareModelCode, Scenario scenario)
        {
            Guid? scenarioId = scenario?.Id;

            using (_db.Open())
                return _db.SoftwareModels.LinqReader.Where(m => m.ExternalId == softwareModelCode && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a device model by a given device, software model and scenario or null if  no matchin record exists.
        /// </summary>
        public DeviceModelDependency GetDeviceModelDependency(Device device, SoftwareModel softwareModel, Scenario scenario)
        {
            Guid? scenarioId = scenario?.Id;
            Guid? deviceModelId = device?.DeviceModelId;
            Guid? softwareModelId = softwareModel?.Id;

            using (_db.Open())
                return _db.DeviceModelDependencies.LinqReader
                    .Where(m => m.DeviceModelId == deviceModelId && m.SoftwareModelDependencyId == softwareModelId && m.ScenarioId == scenarioId)
                    .FirstOrDefault();
        }

        /// <summary>
        ///     Gets the stakeholder login by its external id and scenario.
        /// </summary>
        public StakeholderLogin GetLoginByCode(string externalId, Guid? scenarioId)
        {
            using (_db.Open())
                return _db.StakeholderLogins.LinqReader
                    .Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId)
                    .FirstOrDefault();
        }
    }
}