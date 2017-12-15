using FutureState.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain
{
    /// <summary>
    ///     An architectural concern or domain. A field of expertise that is managed.
    /// </summary>
    public class DesignDomain : FSEntity, IEntityMutableKey<Guid>
    {
        [StringLength(100)]
        public string Type { get; set; }

        /// <summary>
        ///     Gets the containing domain.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        ///     Gets the domain's external id.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets/sets the date the domain was created.
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        ///     Gets/sets the user that last modified the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets/sets the date the user was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DesignDomain()
        {
            // parameterless constructor required by serializer
        }

        /// <summary>
        ///     Creates a new instance with a given display name and description.
        /// </summary>
        /// <param name="displayName">The display name of the domain.</param>
        /// <param name="description">The description of the domain.</param>
        public DesignDomain(string displayName, string description = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(displayName, nameof(displayName));

            Id = SeqGuid.Create();

            DateCreated = DateTime.UtcNow;
            DisplayName = displayName;
            Description = description ?? "";
        }

        /// <summary>
        ///     Model service extensions/
        /// </summary>
        public DomainService Services { get; private set; }

        /// <summary>
        ///     Assigns the implementation of <see cref="DomainServices"/>.
        /// </summary>
        /// <param name="service">The service implementation.</param>
        public void SetServices(DomainService service) => Services = service;

        public class DomainService 
        {
            private readonly Func<DesignDomain> _getParent;
            private readonly Func<IEnumerable<Policy>> _getPolicies;

            public DomainService(Func<DesignDomain> getParent, Func<IEnumerable<Policy>> getPolicies)
            {
                Guard.ArgumentNotNull(getParent, nameof(getParent));
                Guard.ArgumentNotNull(getPolicies, nameof(getPolicies));

                _getParent = getParent;
                _getPolicies = getPolicies;
            }

            public DesignDomain GetParent() => _getParent();

            public IEnumerable<Policy> GetPolicies() => _getPolicies();

            public IEnumerable<Policy> GetPolicies(DateTime date)
            {
                var activatePolicies = GetPolicies()
                    .Where(m => m.DateAdded <= date && (m.DateRemoved == null || m.DateRemoved > date));

                return activatePolicies;
            }
        }
    }
}
