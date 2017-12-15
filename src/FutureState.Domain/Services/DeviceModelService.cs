using FutureState.Data.Keys;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Services to add, remove and view properties of device models e.g. reference workstations, servers as
    ///     well as firewalls.
    /// </summary>
    public class DeviceModelService : FsService<DeviceModel>
    {
        readonly FsUnitOfWork _sfDb;
        readonly SoftwareModelService _softwareModelService;
        private readonly StakeholderService _stakeholdersService;
        private readonly DesignDomainService _domainProvider;
        private readonly ScenarioProvider _scenarioProvider;
        private readonly ProjectService _projectService;
        private readonly CapabilityProvider _capabilityService;

        public DeviceModelService(
            FsUnitOfWork db,
            FSSecurityContext securityContext,
            ReferenceProvider referenceService,
            CapabilityProvider capabilityService,
            ProjectService projectService,
            StakeholderService stakeholdersService,
            SoftwareModelService softwareModelService, 
            DesignDomainService domainService,
            ScenarioProvider scenarioProvider,
            IEntityIdProvider<DeviceModel, Guid> idProvider)
            : base(securityContext, referenceService, db, idProvider, softwareModelService?.MessagePipe)
        {
            Guard.ArgumentNotNull(softwareModelService, nameof(softwareModelService));

            _sfDb = db; // null reference checked by base class
            _softwareModelService = softwareModelService;
            _stakeholdersService = stakeholdersService;
            _projectService = projectService;
            _domainProvider = domainService;
            _scenarioProvider = scenarioProvider;
            _capabilityService = capabilityService;
        }

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetScenario(string scenarioCode)
        {
            return _projectService.GetScenario(scenarioCode);
        }

        /// <summary>
        ///     Gets a device model by an external id and a given scenario code.
        /// </summary>
        /// <param name="externalId">The device's surrogate key.</param>
        public DeviceModel GetByExternalId(string externalId)
        {
            return GetByExternalId(externalId, (Guid?)null);
        }

        /// <summary>
        ///     Gets all life device models that are in a particular life cycle stage as of a given date.
        /// </summary>
        public IEnumerable<DeviceModel> GetByLifeCycleStage(string lifeCycleId, DateTime dateTime)
        {
            return Where(m => m.LifeCycleStageDate <= dateTime && m.LifeCycleId == lifeCycleId);
        }

        /// <summary>
        ///     Gets a device model by external id and scenario code.
        /// </summary>
        /// <param name="externalId">The device's surrogate key.</param>
        /// <param name="scenarioId">The design scenario the artefact belongs to.</param>
        /// <returns></returns>
        public DeviceModel GetByExternalId(string externalId, Guid? scenarioId)
        {
            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a device model by an external id and a given scenario code.
        /// </summary>
        /// <param name="externalId">The device's surrogate key.</param>
        /// <param name="scenarioCode">The design scenario the artefact belongs to.</param>
        /// <returns></returns>
        public DeviceModel GetByExternalId(string externalId, string scenarioCode)
        {
            if(string.IsNullOrWhiteSpace(scenarioCode))
                return Where(m => m.ExternalId == externalId && m.ScenarioId == null).FirstOrDefault();

            Scenario scenario = _projectService.GetScenario(scenarioCode);

            Guid? scenarioId = scenario?.Id;

            return Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();
        }

        /// <summary>
        ///     Gets a stakeholder by its external id as well as a scenario id.
        /// </summary>
        /// <returns></returns>
        public Stakeholder GetStakeholder(string stakeholderExternalId, Guid? scenarioId)
        {
            return _stakeholdersService.GetByExternalId(stakeholderExternalId, scenarioId);
        }

        /// <summary>
        ///     Gets the architectural deomain that the given design artefact belongs to.
        /// </summary>
        /// <param name="deviceModel">The device model to resolve th domain for.</param>
        /// <returns></returns>
        public DesignDomain GetDomain(DeviceModel deviceModel)
        {
            Guard.ArgumentNotNull(deviceModel, nameof(deviceModel));

            if (!deviceModel.DomainId.HasValue)
                return null;

            return _domainProvider.GetById(deviceModel.DomainId.Value);
        }

        /// <summary>
        ///     Gets the design domain by its external key.
        /// </summary>
        public DesignDomain GetDomain(string domainExternalId)
        {
            return _domainProvider.GetByExternalId(domainExternalId);
        }

        /// <summary>
        ///     Gets a software model associated with an external key and scenario id.
        /// </summary>
        public SoftwareModel GetSoftwareModel(string softwareModelExternalId, Guid? scenarioId)
        {
            return _softwareModelService.GetByExternalId(softwareModelExternalId, scenarioId);
        } 

        /// <summary>
        ///     Removes all software model dependencies linked to a device model record.
        /// </summary>
        public void RemoveDependencies(Guid deviceModelId)
        {
            using (_sfDb.Open())
            {
                _sfDb.DeviceModelDependencies.BulkDeleter.Delete(m => m.DeviceModelId == deviceModelId);

                _sfDb.Commit();
            }
        }

        protected override void OnBeforeDelete(Guid key)
        {
            // db is open

            // delete dependencies
            _sfDb.DeviceModelDependencies.BulkDeleter.Delete(m => m.DeviceModelId == key);

            // before delete
            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Associated software as a dependency of a given device model.
        /// </summary>
        public void AddSoftwareDependencies(DeviceModel deviceModel, SoftwareModel softwareModel, string displayName, string description = null)
        {
            // create dependency
            var dependency = new DeviceModelDependency(deviceModel, softwareModel, displayName, description);

            using (_sfDb.Open())
            {
                _sfDb.DeviceModelDependencies.Writer.Insert(dependency);

                _sfDb.Commit();
            }
        }

        /// <summary>
        ///     Registers a set of dependencies.
        /// </summary>
        public void AddDependencies(IEnumerable<DeviceModelDependency> dependencies)
        {
            Guard.ArgumentNotNull(dependencies, nameof(dependencies));

            using (_sfDb.Open())
            {
                _sfDb.DeviceModelDependencies.Writer.Insert(dependencies);

                _sfDb.Commit();
            }
        }

        public void UpdateDependencies(IEnumerable<DeviceModelDependency> dependencies)
        {
            using (_sfDb.Open())
            {
                _sfDb.DeviceModelDependencies.Writer.Insert(dependencies);

                _sfDb.Commit();
            }
        }

        /// <summary>
        ///     Gets all the software models associated to a given device model.
        /// </summary>
        public IEnumerable<DeviceModelDependency> GetDependencies(DeviceModel deviceModel)
        {
            Guard.ArgumentNotNull(deviceModel, nameof(deviceModel));

            ICollection<DeviceModelDependency> dependencies;

            using (_sfDb.Open())
            {
                dependencies = _sfDb.DeviceModelDependencies
                    .LinqReader.Where(m => m.DeviceModelId == deviceModel.Id)
                    .ToCollection();
            }

            IEnumerable<SoftwareModel> models = _softwareModelService.GetByIds(dependencies.Select(m => m.SoftwareModelDependencyId));

            //initialize
            dependencies.Each(m =>
            {
                var services = new DeviceModelDependency.DomainServices(
                    () => models.FirstOrDefault(n => n.Id == m.SoftwareModelDependencyId),
                    () => deviceModel
                );

                // set 
                m.SetServices(services);
            });

            return dependencies;
        }

        // gets all interfaces associated with a given device model id
        /// <summary>
        ///     Gets all the interfaces registered against a device model identified by a given device model id.
        /// </summary>
        public IEnumerable<SoftwareModelInterface> GetInterfaces(Guid deviceModelId)
        {
            IList<Guid> softwareModelIds = null;

            using (_sfDb.Open())
            {
                // software model dependencies ids
                softwareModelIds = _sfDb.DeviceModelDependencies
                    .LinqReader.Where(m => m.DeviceModelId == deviceModelId)
                    .Select(m => m.SoftwareModelDependencyId)
                    .ToList();
            }

            // get list of software models
            List<SoftwareModel> softwareModels = _softwareModelService
                .GetByIds(softwareModelIds).ToList();

            // call after db connection closed
            return
                softwareModels.SelectMany(m => m.Services.GetInterfaces()).ToCollection();
        }

        /// <summary>
        ///     Initializes a device model to the service,
        /// </summary>
        public override DeviceModel Initialize(DeviceModel entity)
        {
            // attach services to the device model  context
            entity.SetServices(new DeviceModel.DomainServices(
                () => GetInterfaces(entity.Id),
                () => GetDependencies(entity),
                () => _capabilityService.GetCapabilities(entity.Id),
                () => GetReferencesProvider().GetReferences(entity.Id),
                () => GetDomain(entity),
                () => entity.ScenarioId.HasValue ? _scenarioProvider.GetById(entity.ScenarioId.Value) : null
            ));

            return base.Initialize(entity);
        }
    }
}