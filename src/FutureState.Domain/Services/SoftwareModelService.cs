using FutureState.ComponentModel;
using FutureState.Data;
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
    ///     Create, model and delete software model instances.
    /// </summary>
    public class SoftwareModelService : FsService<SoftwareModel>, IService
    {
        private readonly FsUnitOfWork _sfDb;
        private readonly CapabilityProvider _capabilitiesProvider;
        private readonly SoftwareModelInterfaceService _softwareModelInterfaceService;
        private readonly ProviderLinq<SoftwareModelDependency, Guid> _softwareModelDependencyService;
        private readonly ProjectService _projectService;
        private readonly StakeholderService _stakeholderService;
        private readonly BusinessUnitProvider _businessUnitService;

        public SoftwareModelService(
            FsUnitOfWork db,
            FSSecurityContext securityContext,
            ReferenceProvider referenceService,
            BusinessUnitProvider businessUnitService,
            CapabilityProvider capabilityProvider,
            ProjectService projectService,
            StakeholderService stakeholderService,
            SoftwareModelInterfaceService softwareModelInterfaceService,
            ProviderLinq<SoftwareModelDependency,Guid> softwareModelDependencyProvider,
            IEntityIdProvider<SoftwareModel, Guid> idProvider,
            IMessagePipe messagePipe = null,
            IProvideSpecifications<SoftwareModel> specProvider = null,
            EntityHandler<SoftwareModel,Guid> entityHandler = null)
            : base(securityContext, referenceService, db, idProvider, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(securityContext, nameof(securityContext));
            Guard.ArgumentNotNull(capabilityProvider, nameof(capabilityProvider));
            Guard.ArgumentNotNull(referenceService, nameof(referenceService));
            Guard.ArgumentNotNull(softwareModelInterfaceService, nameof(softwareModelInterfaceService));

            _sfDb = db;

            _capabilitiesProvider = capabilityProvider;
            _businessUnitService = businessUnitService;
            _stakeholderService = stakeholderService;
            _softwareModelInterfaceService = softwareModelInterfaceService;
            _softwareModelDependencyService = softwareModelDependencyProvider;
            _projectService = projectService;
        }

        protected override void OnBeforeAdd(SoftwareModel entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }

        /// <summary>
        ///     Gets a business unit by its external key.
        /// </summary>
        public BusinessUnit GetBusinessUnitByCode(string businessUnitCode) => _businessUnitService.GetByExternalId(businessUnitCode);

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetScenario(string scenarioCode) => _projectService.GetScenario(scenarioCode);

        /// <summary>
        ///     Removes a dependency from a given software model by a given model id and its dependency id.
        /// </summary>
        public void RemoveDependency(Guid containerId, Guid softwareModelId)
        {
            using (_sfDb.Open())
            {
                // will only be one record per container/model combination
                Guid? dependencyId = _sfDb.SoftwareModelDependencies.LinqReader
                    .Where(m => m.SoftwareModelId == containerId && m.SoftwareModelDependencyId == softwareModelId)
                    .Select(m => m.Id)
                    .FirstOrDefault();

                if (dependencyId.HasValue)
                {
                    _softwareModelDependencyService.RemoveById(dependencyId.Value, _sfDb);
                }
            }
        }

        /// <summary>
        ///     Gets any errors associated with the state of a given capability.
        /// </summary>
        /// <param name="capability"></param>
        /// <returns></returns>
        public IEnumerable<Error> Validate(Capability capability) => _capabilitiesProvider.Validate(capability);

        /// <summary>
        ///     Gets a software model by its external key belonging to the default scenario.
        /// </summary>
        public SoftwareModel GetByExternalId(string externalId) => GetByExternalId(externalId, null);

        public Stakeholder GetStakeholder(string stakeholderExternalId, Guid? scenarioId) => this._stakeholderService.GetByExternalId(stakeholderExternalId, scenarioId);

        public SoftwareModel GetByExternalId(string externalId, Guid? scenarioId) => Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();

        protected override void OnBeforeDelete(Guid key)
        {
            base.OnBeforeDelete(key);

            // database connection is opened

            // delete all dependencies
            _sfDb.Capabilities.BulkDeleter.Delete(m => m.SoftwareModelId == key);
            _sfDb.References.BulkDeleter.Delete(m => m.ReferenceId == key);
            _sfDb.SoftwareModelInterfaces.BulkDeleter.Delete(m => m.SoftwareModelId == key);
            _sfDb.SoftwareModelDependencies.BulkDeleter.Delete(m => m.SoftwareModelId == key);
        }

        /// <summary>
        ///     Adds an array of software model dependencies to the system.
        /// </summary>
        public void AddDependencies(params SoftwareModelDependency[] dependencies)
        {
            using (_sfDb.Open())
            {
                _sfDb.SoftwareModelDependencies.Writer.Insert(dependencies);
                _sfDb.Commit();
            }
        }

        /// <summary>
        ///     Adds a set of software model interfaces devices can potentially connect to.
        /// </summary>
        public void AddInterfaces(IEnumerable<SoftwareModelInterface> interfaces)
        {
            using (_sfDb.Open())
            {
                _sfDb.SoftwareModelInterfaces.Writer.Insert(interfaces);
                _sfDb.Commit();
            }
        }

        /// <summary>
        ///     Remove all software model interfaces linked to a software model.
        /// </summary>
        public void RemoveInterfaces(Guid softwareModelId)
        {
            _softwareModelInterfaceService.Remove(m => m.SoftwareModelId == softwareModelId);
        }

        /// <summary>
        ///     Remove all dependencies associated with a given software model id.
        /// </summary>
        public void RemoveDependencies(Guid softwareModelId)
        {
            _softwareModelDependencyService.Remove(m => m.SoftwareModelId == softwareModelId);
        }

        /// <summary>
        ///     Attaches the entity to the service.
        /// </summary>
        public override SoftwareModel Initialize(SoftwareModel entity)
        {
            entity.SetSevices(new SoftwareModel.DomainServices(
                () => GetReferencesProvider().GetReferences(entity.Id),
                () => GetInterfaces(entity.Id),
                () => GetCapabilities(entity.Id),
                () => GetSoftwareDependencies(entity.Id)
            ));

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets all capabilities associated with a software model identified by its key.
        /// </summary>
        public IList<Capability> GetCapabilities(Guid softwareModelId)
        {
            return this._capabilitiesProvider.GetCapabilities(softwareModelId);
        }

        /// <summary>
        ///     Gets the software models that a given software model depends on.
        /// </summary>
        /// <param name="softwareModelId">The id of the software model to evaluate the dependencies for.</param>
        /// <returns></returns>
        public ICollection<SoftwareModel> GetDependenciesAsModels(Guid softwareModelId)
        {
            ICollection<SoftwareModel> dependencies;
            ICollection<Guid> ids;

            using (_sfDb.Open())
            {
                // get the ids of all the depencies
                ids = _sfDb.SoftwareModelDependencies.LinqReader.Where(m => m.SoftwareModelId == softwareModelId)
                    .Select(m => m.SoftwareModelDependencyId)
                    .ToCollection();

                // get the list of sub-dependencies
                dependencies = _sfDb.SoftwareModels.LinqReader.GetByIds(ids)
                    .ToCollection();
            }

            // initialize all dependencies
            foreach (SoftwareModel dependency in dependencies)
                Initialize(dependency);

            return dependencies;
        }

        /// <summary>
        ///     Gets all dependencies against the software model.
        /// </summary>
        public IEnumerable<SoftwareModelDependency> GetDependencies(Guid softwareModelId)
        {
            return _softwareModelDependencyService
                .Where(m => m.SoftwareModelId == softwareModelId).ToList();
        }

        /// <summary>
        ///     Gets all apps a given software model depends on.
        /// </summary>
        public IEnumerable<SoftwareModel> GetSoftwareDependencies(Guid softwareModelId)
        {
            var ids = _softwareModelDependencyService
                .Where(m => m.SoftwareModelId == softwareModelId)
                .Select(m => m.SoftwareModelDependencyId).ToList();

            return GetByIds(ids);
        }

        /// <summary>
        ///     Registers a dependency to a given software model.
        /// </summary>
        public void AddDependency(SoftwareModel container, SoftwareModel softwareModel, string description = null)
        {
            Guard.ArgumentNotNull(container, nameof(container));
            Guard.ArgumentNotNull(softwareModel, nameof(softwareModel));

            var dependency = new SoftwareModelDependency(container, softwareModel, description);

            if (dependency.SoftwareModelId == dependency.SoftwareModelDependencyId)
                throw new RuleException("Cannot register self as a dependency.");

            _softwareModelDependencyService.Add(dependency);
        }

        /// <summary>
        ///     Add a set of software model dependencies with a common description.
        /// </summary>
        public void AddDependencies(SoftwareModel container, IEnumerable<SoftwareModel> softwareModels, string description = null)
        {
            Guard.ArgumentNotNull(container, nameof(container));
            Guard.ArgumentNotNull(softwareModels, nameof(softwareModels));

            var dependenciesToAdd = new List<SoftwareModelDependency>();

            softwareModels.Each(m =>
            {
                var dependency = new SoftwareModelDependency(container, m, description);

                if (dependency.SoftwareModelId == dependency.SoftwareModelDependencyId)
                    throw new RuleException("Cannot register self as a dependency.");

                dependenciesToAdd.Add(dependency);
            });
            
            _softwareModelDependencyService.Add(dependenciesToAdd);
        }

        /// <summary>
        ///     Gets all life software models that are in a particular life cycle stage as of a given date.
        /// </summary>
        public IEnumerable<SoftwareModel> GetByLifeCycleStage(string lifeCycleId, DateTime dateTime)
        {
            return Where(m => m.LifeCycleStageDate <= dateTime && m.LifeCycleId == lifeCycleId);
        }

        /// <summary>
        ///     Registers an interface against a given software model.
        /// </summary>
        public void AddInterface(SoftwareModelInterface modelInterface)
        {
            _softwareModelInterfaceService.Add(modelInterface);
        }

        /// <summary>
        ///     Updates a given software interface.
        /// </summary>
        public void UpdateInterface(SoftwareModelInterface modelInterface)
        {
            _softwareModelInterfaceService.Update(modelInterface);
        }

        /// <summary>
        ///     Removes a softare model interface by software model id and interface id.
        /// </summary>
        public void RemoveInterface(Guid ssoftwareModelInterfaceId)
        {
            _softwareModelInterfaceService.RemoveById(ssoftwareModelInterfaceId);
        }

        /// <summary>
        ///     Gets all interfaces registered with a software model identified by its id.
        /// </summary>
        /// <param name="softwareModelId">
        ///     The software model id to query for.
        /// </param>
        public IList<SoftwareModelInterface> GetInterfaces(Guid softwareModelId)
        {
            return _softwareModelInterfaceService.Where(m => m.SoftwareModelId == softwareModelId).ToList();
        }

        /// <summary>
        ///     Removes all capabilities associated with a given software model.
        /// </summary>
        public void RemoveCapabilities(Guid softwareModelId)
        {
            _softwareModelDependencyService.Remove(m => m.SoftwareModelId == softwareModelId);
        }

        /// <summary>
        ///     Records a set of capabilities against a given software model.
        /// </summary>
        public void FlushFill(SoftwareModel entity, IEnumerable<Capability> capabilities)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));
            Guard.ArgumentNotNull(capabilities, nameof(capabilities));

            string userName = GetCurrentUser();

            capabilities.Each(m =>
            {
                m.ScenarioId = entity.ScenarioId;

                if(string.IsNullOrWhiteSpace(m.UserName))
                    m.UserName = userName;
            });

            _capabilitiesProvider.FlushFill(capabilities);
        }

        /// <summary>
        ///     Gets a capability by its external key and a scenario.
        /// </summary>
        public Capability GetCapabilityById(string capabilityExternalId, Guid? scenarioId)
        {
            return _capabilitiesProvider.GetByExternalId(capabilityExternalId, scenarioId);
        }

        /// <summary>
        ///     Batch update a set of capabilities to add and/or update.
        /// </summary>
        public void AddUpdateCapabilities(IEnumerable<Capability> capabilitiesToAdd, IEnumerable<Capability> capabilitiesToUpdate)
        {
            Guard.ArgumentNotNull(capabilitiesToAdd, nameof(capabilitiesToAdd));
            Guard.ArgumentNotNull(capabilitiesToUpdate, nameof(capabilitiesToUpdate));

            _capabilitiesProvider.AddUpdate(capabilitiesToAdd, capabilitiesToUpdate);
        }
    }
}