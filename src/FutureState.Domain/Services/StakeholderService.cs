using FutureState.ComponentModel;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Domain.Providers;
using FutureState.Security;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Service to add, remove and query stakeholders.
    /// </summary>
    public class StakeholderService : FsService<Stakeholder>
    {
        readonly FsUnitOfWork _db;

        private readonly ProjectService _projectService;
        private readonly StakeholderLoginProvider _stakeholderLoginProvider;
        private readonly BusinessUnitProvider _businessUnitProvider;

        //.ctor
        public StakeholderService(
            FSSecurityContext securityContext,
            ProjectService projectService,
            BusinessUnitProvider businessUnitProvider, 
            StakeholderLoginProvider stakeholderLoginProvider,
            ReferenceProvider referencesProvider,
            FsUnitOfWork db,
            IMessagePipe messagePipe,
            IEntityIdProvider<Stakeholder, Guid> idProvider,
            IProvideSpecifications<Stakeholder> specProvider = null,
            EntityHandler<Stakeholder,Guid> entityHandler = null)
            : base(securityContext, referencesProvider, db, idProvider, messagePipe, specProvider, entityHandler)
        {
            Guard.ArgumentNotNull(projectService, nameof(projectService));
            Guard.ArgumentNotNull(stakeholderLoginProvider, nameof(stakeholderLoginProvider));

            _db = db;
            _projectService = projectService;
            _businessUnitProvider = businessUnitProvider;
            _stakeholderLoginProvider = stakeholderLoginProvider;
        }

        /// <summary>
        ///     Gets the stakeholder that matches the given external id.
        /// </summary>
        public Stakeholder GetByExternalId(string externalId) => GetByExternalId(externalId, null);

        protected override void OnBeforeAdd(Stakeholder entity)
        {
            if (string.IsNullOrWhiteSpace(entity.UserName))
                entity.UserName = GetCurrentUser();

            base.OnBeforeAdd(entity);
        }

        public StakeholderLogin GetLoginByCode(string externalId, Guid? scenarioId)
        {
            return _stakeholderLoginProvider.GetByExternalId(externalId, scenarioId); 
        }

        /// <summary>
        ///     Registers a stakeholder login.
        /// </summary>
        public void AddLogin(StakeholderLogin stakeholderLogin)
        {
            _stakeholderLoginProvider.Add(stakeholderLogin);
        }

        /// <summary>
        ///     Updates a set of login entries.
        /// </summary>
        public void UpdateLogins(IEnumerable<StakeholderLogin> logins)
        {
            _stakeholderLoginProvider.Update(logins);
        }

        /// <summary>
        ///     Adds a set of logins.
        /// </summary>
        public void AddUpateLogins(IEnumerable<StakeholderLogin> logins, IEnumerable<StakeholderLogin> loginsToUpdate = null)
        {
            _stakeholderLoginProvider.AddUpdate(logins, loginsToUpdate ?? new StakeholderLogin[0]);
        }

        /// <summary>
        ///     Deletes/remove a stackholder.
        /// </summary>
        public void Remove(Stakeholder stakeholder)
        {
            Guard.ArgumentNotNull(stakeholder, nameof(stakeholder));

            RemoveById(stakeholder.Id);
        }

        protected override void OnBeforeDelete(Guid key)
        {
            _stakeholderLoginProvider.Remove(m => m.StakeholderId == key);

            _db.UserGroupMemberships.BulkDeleter.Delete(m => m.GroupId == key || m.MemberId == key);

            _db.EntitySet.Writer.DeleteById(key);

            GetReferencesProvider().RemoveReferences(key, _db);

            base.OnBeforeDelete(key);
        }

        /// <summary>
        ///     Gets a stakeholder by its external id and scenario.
        /// </summary>
        public Stakeholder GetByExternalId(string externalId, Guid? scenarioId) => Where(m => m.ExternalId == externalId && m.ScenarioId == scenarioId).FirstOrDefault();

        /// <summary>
        ///     Gets the logins associated with a given stakeholder.
        /// </summary>
        public IEnumerable<StakeholderLogin> GetLogins(Stakeholder stakeholder)
        {
            Guard.ArgumentNotNull(stakeholder, nameof(stakeholder));

            return _stakeholderLoginProvider.Where(m => m.StakeholderId == stakeholder.Id);
        }

        // finish initializing this entity
        public override Stakeholder Initialize(Stakeholder entity)
        {
            var context = new Stakeholder.DomainServices(
                () => GetLogins(entity), // resolve logins
                () => entity.BusinessUnitId.HasValue ? _businessUnitProvider.GetById(entity.BusinessUnitId.Value) : null,
                () => entity.ScenarioId.HasValue ? _projectService.GetScenarioById(entity.ScenarioId.Value) : null); // resolve scenario

            // set the domain context
            entity.SetSevices( context );

            return base.Initialize(entity);
        }
    }
}