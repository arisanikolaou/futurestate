using FutureState.ComponentModel;
using FutureState.Data;
using FutureState.Data.Keys;
using FutureState.Data.Providers;
using FutureState.Domain.Data;
using FutureState.Services;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain.Services
{
    /// <summary>
    ///     Service to add/remove and view user groups to a given entity.
    /// </summary>
    public class UserGroupService : ProviderLinq<UserGroup, Guid>, IService
    {
        private readonly FsUnitOfWork _db;

        static readonly List<ISpecification<UserGroupMembership>> _membershipRules = new DataAnnotationsSpecProvider<UserGroupMembership>()
            .GetSpecifications().ToList();
        private readonly StakeholderService _stakeholderService;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="db">The references database.</param>
        /// <param name="idProvider"></param>
        /// <param name="messagePipe"></param>
        /// <param name="referencesSpec">Rules to validate a given reference entity.</param>
        public UserGroupService(
            FsUnitOfWork db,
            StakeholderService stakeholderService,
            IEntityIdProvider<UserGroup, Guid> idProvider,
            IMessagePipe messagePipe,
            IProvideSpecifications<UserGroup> referencesSpec = null,
            EntityHandler<UserGroup, Guid> entityHandler = null)
            : base(db, idProvider, messagePipe, referencesSpec, entityHandler)
        {
            Guard.ArgumentNotNull(db, nameof(db));

            _stakeholderService = stakeholderService;
            _db = db;
        }

        public override UserGroup Initialize(UserGroup entity)
        {
            var services = new UserGroup.DomainServices(
                () => GetMembers(entity));

            entity.SetServices(services);

            return base.Initialize(entity);
        }

        /// <summary>
        ///     Gets all members that belong to a given user group.
        /// </summary>
        public IEnumerable<Stakeholder> GetMembers(UserGroup entity)
        {
            Guard.ArgumentNotNull(entity, nameof(entity));

            using (_db.Open())
            {
                List<Guid> ids = _db.UserGroupMemberships
                    .LinqReader.Where(m => m.GroupId == entity.Id)
                    .Select(m => m.MemberId).ToList();

                return _stakeholderService.GetByIds(ids, _db);
            }
        }

        /// <summary>
        ///     Gets a user group by its name and scenario id.
        /// </summary>
        /// <param name="scenarioId">Null indicates default scenario or no scenario.</param>
        /// <returns></returns>
        public UserGroup GetByName(string groupName, Guid? scenarioId) => this
            .Where(m => m.ScenarioId == scenarioId && m.DisplayName == groupName)
            .FirstOrDefault();

         /// <summary>
        ///     Adds member to a group type of stakeholder.
        /// </summary>
        /// <param name="userGroup">The parent containing group.</param>
        /// <param name="member">The member of the group.</param>
        public void AddMember(UserGroup userGroup, Stakeholder member)
        {
            Guard.ArgumentNotNull(userGroup, nameof(userGroup));
            Guard.ArgumentNotNull(member, nameof(member));

            // update parent memberships
            var membership = new UserGroupMembership(userGroup, member);

            SetMembership(new[] { membership });
        }

        /// <summary>
        ///     Validates a given stakeholder membership entity.
        /// </summary>
        public IEnumerable<Error> Validate(UserGroupMembership membership) => _membershipRules.ToErrors(membership);

        /// <summary>
        ///     Flush fills a set of user group memberships.
        /// </summary>
        public void SetMembership(IEnumerable<UserGroupMembership> memberships)
        {
            Guard.ArgumentNotNull(memberships, nameof(memberships));

            // ensure unique
            var membershipList = memberships.ToList();
            var distinctList = new List<UserGroupMembership>();

            for (int i = 0; i < membershipList.Count; i++)
            {
                bool exists = false;
                var subject = membershipList[i];

                for (int x = 1; x < membershipList.Count; x++)
                {
                    var otherMembership = membershipList[x];

                    if (subject.ScenarioId == otherMembership.ScenarioId)
                        if (subject.MemberId == otherMembership.MemberId)
                            if (subject.GroupId == otherMembership.GroupId)
                            {
                                exists = true;
                                break;
                            }
                }

                if (!exists)
                    distinctList.Add(subject);
            }

            // valid membership
            foreach (var membership in distinctList)
            {
                Validate(membership)
                    .ThrowIfExists();

                // TODO: convert to a specification
                // ensure the tree isn't self referencing
                if (membership.GroupId.Equals(membership.MemberId))
                {
                    throw new RuleException("A member cannot be the same as the parent.", new Error[]
                    {
                        new Error("A member cannot be the same as the parent."),
                    });
                }
            }

            using (_db.Open())
            {
                foreach (var membership in distinctList)
                    // find existing records describing the stakeholder's group memberships and delete
                    _db.UserGroupMemberships.BulkDeleter.Delete(m => m.ScenarioId == membership.ScenarioId && m.MemberId == membership.MemberId);

                // insert new member ship records
                _db.UserGroupMemberships.Writer.Insert(distinctList);

                _db.Commit();
            }
        }

        /// <summary>
        ///     Removes all memberships a given stakeholder may have.
        /// </summary>
        public void RemoveGroupMemberships(Stakeholder stakeholder)
        {
            using (_db.Open())
            {
                RemoveGroupMemberships(stakeholder, _db);

                _db.Commit();
            }
        }

        /// <summary>
        ///     Removes all memberships a given stakeholder may have.
        /// </summary>
        public void RemoveGroupMemberships(Stakeholder stakeholder, IUnitOfWorkLinq<UserGroupMembership, Guid> db)
        {
            db.EntitySet.BulkDeleter.Delete(m => m.ScenarioId == stakeholder.ScenarioId && m.MemberId == stakeholder.Id);
        }
    }
}