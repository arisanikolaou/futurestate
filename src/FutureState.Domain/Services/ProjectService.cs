using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Add/remove projects and project scenarios.
    /// </summary>
    public class ProjectService : ProviderLinq<Project, Guid>
    {
        private readonly FsUnitOfWork _db;
        private readonly ScenarioProvider _scenarioProvider;
        private readonly BusinessUnitProvider _businessUnitProvider;

        public ProjectService(
            ScenarioProvider scenarioProvider,
            BusinessUnitProvider businessUnitProvider,
            FsUnitOfWork db,
            IEntityIdProvider<Project, Guid> idProvider,
            IProvideSpecifications<Project> specProvider = null,
            EntityHandler<Project, Guid> entityHandler = null)
            : base(db, idProvider, scenarioProvider?.MessagePipe, specProvider, entityHandler)
        {
            _db = db;

            _scenarioProvider = scenarioProvider;
            _businessUnitProvider = businessUnitProvider;
        }

        /// <summary>
        ///     Gets a project by its external id.
        /// </summary>
        /// <param name="externalId">The external id to search for.</param>
        /// <returns></returns>
        public Project GetByExternalId(string externalId)
        {
            return Where(m => m.ExternalId == externalId).FirstOrDefault();
        }

        /// <summary>
        ///     Removes a project scenario.
        /// </summary>
        /// <param name="scenario">Required. The scenario to remove.</param>
        public void Remove(Scenario scenario)
        {
            Guard.ArgumentNotNull(scenario, nameof(scenario));

            _scenarioProvider.RemoveById(scenario.Id);
        }

        protected override void OnBeforeAdd(Project entity)
        {
            // fixup user
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }

        /// <summary>
        ///     Records a set of scenarios.
        /// </summary>
        public void AddScenarios(IEnumerable<Scenario> scenarios)
        {
            this._scenarioProvider.Add(scenarios);
        }

        /// <summary>
        ///     Returns any errors in the state of a given scenario.
        /// </summary>
        public IEnumerable<Error> Validate(Scenario scenario)
        {
            return _scenarioProvider.Validate(scenario);
        }

        /// <summary>
        ///     Adds a scenario to a project.
        /// </summary>
        public void Add(Project project, Scenario scenario)
        {
            Guard.ArgumentNotNull(project, nameof(project));
            Guard.ArgumentNotNull(scenario, nameof(scenario));

            //assign the project to the
            scenario.ProjectId = project.Id;

            if (string.IsNullOrEmpty(scenario.UserName))
                scenario.UserName = GetCurrentUser();

            // attach to serviec
            Initialize(project);

            _scenarioProvider.Add(scenario);
        }

        /// <summary>
        ///     Adds/updates a set of scenarios.
        /// </summary>
        public void AddUpdate(List<Scenario> added, List<Scenario> updated)
        {
            _scenarioProvider.AddUpdate(added, updated);
        }

        /// <summary>
        ///     Gets all scenarios associated with a given project.
        /// </summary>
        public IEnumerable<Scenario> GetScenarios(Guid projectId)
        {
            return _scenarioProvider.Where(m => m.ProjectId == projectId);
        }

        /// <summary>
        ///     Initializes a given project.
        /// </summary>
        public override Project Initialize(Project entity)
        {
            entity.SetServices(new Project.DomainServices(
                () => GetBusinessUnit(entity),
                () => GetScenarios(entity.Id).ToList()));

            return base.Initialize(entity);
        }

        public BusinessUnit GetBusinessUnit(Project entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            return entity.BusinessUnitId.HasValue ? _businessUnitProvider.GetById(entity.BusinessUnitId.Value) : null;
        }

        protected override void OnBeforeDelete(Guid key)
        {
            // connection open
            _scenarioProvider.DeleteByProject(key, _db);

            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Gets a scenario by its external key.
        /// </summary>
        public Scenario GetScenario(string scenarioExternalId)
        {
            return _scenarioProvider.GetByExternalId(scenarioExternalId);
        }

        /// <summary>
        ///     Gets a scenario by its system id.
        /// </summary>
        public Scenario GetScenarioById(Guid id)
        {
            return _scenarioProvider.GetById(id);
        }

    }
}