using FutureState.Data;
using System;
using System.Collections.Generic;

namespace FutureState.Domain
{
    /// <summary>
    ///     Policies communicate guiding architectural standards that are applicable 
    ///     to a given design domain.
    /// </summary>
    /// <remarks>
    ///     The strength of a policy is determined by the number of people endorsing it as well
    ///     as how current the policy was reviewed.
    ///     Polices relate to a particular design or architectural domain such as 
    ///     web, desktop, software development.
    ///     Policies should be viewed as a set of current best practices designing for a given
    ///     domain such as sql server, databases, web, or back end.
    /// </remarks>
    public class Policy : FSEntity, IEntityMutableKey<Guid>, IAuditable
    {
        /// <summary>
        ///     Gets the containing policy id.
        /// </summary>
        public Guid? ContainerId { get; set; }

        /// <summary>
        ///     Gets/sets the external id for the policy.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the business unit the policy applies to. If null the policy applies to all.
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <summary>
        ///     Gets the date the policy was added in utc.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the date the policy was retired.
        /// </summary>
        public DateTime? DateRemoved { get; set; }

        /// <summary>
        ///     Gets/sets the user that last modified the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the number of parties that have endorse this policy.
        /// </summary>
        public int? Votes { get; set; }

        /// <summary>
        ///     Gets the date the policy was last reviewed.
        /// </summary>
        public DateTime? LastReviewDate { get; set; }

        /// <summary>
        ///     Gets the last review notes, if any.
        /// </summary>
        public string LastReviewNotes { get; set; }

        /// <summary>
        ///     Gets the design domain that this policy applies to.
        /// </summary>
        public Guid? DesignDomainId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Policy()
        {
            // required by the serializer
        }

        /// <summary>
        ///     Creates a new instance with a given display name and description.
        /// </summary>
        public Policy(string externalId, string displayName, string description)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(externalId, nameof(externalId));
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(displayName, nameof(displayName));

            this.Id = SeqGuid.Create();

            this.DisplayName = displayName ?? "";
            this.Description = description ?? "";
            this.ExternalId = externalId ?? "";
            this.DateAdded = DateTime.UtcNow;
            this.DateLastModified = this.DateAdded;
            this.UserName = "";
        }

        /// <summary>
        ///     Model service extensions.
        /// </summary>
        public DomainServices Services { get; private set; }

        /// <summary>
        ///     Assigns the implementation of <see cref="DomainServices"/>.
        /// </summary>
        /// <param name="service">The service implementation.</param>
        public void SetServices(DomainServices service) => Services = service;

        /// <summary>
        ///     Sets of service that can optionally be attached to the 
        ///     policy.
        /// </summary>
        public class DomainServices
        {
            private readonly Func<IEnumerable<Reference>> _referencesGet;
            private readonly Func<IEnumerable<Policy>> _policiesGet;
            private readonly Func<DesignDomain> _designDomainGet;

            public DomainServices(
                Func<IEnumerable<Reference>> referencesGet,
                Func<IEnumerable<Policy>> policiesGet,
                Func<DesignDomain> designDomainGet)
            {
                Guard.ArgumentNotNull(referencesGet, nameof(referencesGet));
                Guard.ArgumentNotNull(policiesGet, nameof(policiesGet));
                Guard.ArgumentNotNull(designDomainGet, nameof(designDomainGet));

                this._referencesGet = referencesGet;
                this._policiesGet = policiesGet;
                this._designDomainGet = designDomainGet;
            }

            /// <summary>
            ///     Gets references to the policy.
            /// </summary>
            public IEnumerable<Reference> GetReferences() => _referencesGet();

            /// <summary>
            ///     Gets all child policies contained in the current instance.
            /// </summary>
            public IEnumerable<Policy> GetChildPolicies() => _policiesGet();

            /// <summary>
            ///     Gets a design domain that contains this policy.
            /// </summary>
            public DesignDomain GetDesignDomain() => _designDomainGet();
        }
    }
}