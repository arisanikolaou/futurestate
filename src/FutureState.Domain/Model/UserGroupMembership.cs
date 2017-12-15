using FutureState.Data;
using FutureState.Specifications;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     A record of a stakeholder's membership to a given group.
    /// </summary>
    public class UserGroupMembership : IEntityMutableKey<Guid>, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the id of the stakeholder.
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        ///     Gets the stakeholder the owns the relationship.
        /// </summary>
        [NotEmptyGuid("Member id cannot be null or empty.")]
        public Guid MemberId { get; set; }
        /// <summary>
        ///     Gets the stakeholder that the member belongs to.
        /// </summary>
        [NotEmptyGuid("Group id cannot be null or empty.")]
        public Guid GroupId { get; set; }
        /// <summary>
        ///     Gets the scenario the owns the architectural artefact.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the date the entry was created.
        /// </summary>
        [NotEmptyDate("Date Added cannot be null or empty.")]
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the name of the user that last modifeid the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public UserGroupMembership()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="group">The group that owns the membership.</param>
        /// <param name="member">The group that the stakeholder belongs to.</param>
        public UserGroupMembership(UserGroup group, Stakeholder member)
        {
            Guard.ArgumentNotNull(group, nameof(group));
            Guard.ArgumentNotNull(member, nameof(member));

            if (group.ScenarioId != member.ScenarioId)
                throw new InvalidOperationException("Group and member must belong to the same scenario.");

            Id = SeqGuid.Create();

            ScenarioId = group.ScenarioId;
            GroupId = group.Id;
            MemberId = member.Id;
            DateAdded = DateTime.UtcNow;
            UserName = "";
            DateLastModified = DateAdded;
        }
    }
}