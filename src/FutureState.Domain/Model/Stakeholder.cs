using FutureState.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     A stakeholder is any person that can either own or access information systems.
    /// </summary>
    public class Stakeholder : FSEntity, IEntityMutableKey<Guid>, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets the default system admin account.
        /// </summary>
        public static readonly Stakeholder Admin = new Stakeholder()
        {
            Id = Guid.Parse("a838689d-8f86-4038-9327-a6a5f6deb736"), // todo: get rid of magic admin id and pull from config / installer file
            DisplayName = "Admin",
            FirstName = "",
            LastName = "",
            DateCreated = DateTime.UtcNow,
            Description = "",
            StakeholderTypeId = "User"
        };

        /// <summary>
        ///     Gets/sets the scenario the asset belongs to.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the date the record was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the date the stakeholder account was expired.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        ///     Gets the external id of the stakeholder.
        /// </summary>
        [MaxLength(100)]
        [Required(ErrorMessage = "External id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets/sets the type of stakeholder id.
        /// </summary>
        public string StakeholderTypeId { get; set; }

        /// <summary>
        ///     Gets the business unit the stakeholder belongs to.
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <summary>
        ///     Gets the stakeholder's first name.
        /// </summary>
        [StringLength(100)]
        public string FirstName { get; set; }

        /// <summary>
        ///     Gets the stakeholder's last name.
        /// </summary>
        [StringLength(100)]
        public string LastName { get; set; }

        /// <summary>
        ///     Gets/sets user defined attributes for the entity.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Gets the user that last modified the record.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the record was last modified in utc.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Stakeholder()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Stakeholder(string externalId, string displayName = null, Scenario scenario = null) :
            this(externalId, "", "", displayName, scenario)
        {
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Stakeholder(string externalId, string firstName, string lastName, string displayName = null, Scenario scenario = null)
        {
            this.Id = SeqGuid.Create();

            this.FirstName = firstName ?? "";
            this.LastName = lastName ?? "";
            this.DateCreated = DateTime.UtcNow;
            // reserved for future use
            this.DisplayName = displayName ?? $"{FirstName} {LastName}";
            this.StakeholderTypeId = "User";
            this.UserName = "";
            this.ExternalId = externalId ?? "";
            this.DateLastModified = this.DateCreated;
            this.Description = "";

            if (scenario != null)
                ScenarioId = scenario.Id;
        }
        
        /// <summary>
        ///     Gets whether the stakeholder is active.
        /// </summary>
        /// <returns></returns>
        public bool GetIsActive() => !this.DateExpired.HasValue || this.DateExpired.Value < DateTime.UtcNow;

        /// <summary>
        ///     Model service extensions.
        /// </summary>
        public DomainServices Services { get; private set ;}

        /// <summary>
        ///     Assigns domain service extensions to the current instance.
        /// </summary>
        public void SetSevices(DomainServices services)
        {
            this.Services = services;
        }

        // default generic domain context
        public class DomainServices 
        {
            private readonly Func<IEnumerable<StakeholderLogin>> _getLogins;
            private readonly Func<Scenario> _getScenario;
            private readonly Func<BusinessUnit> _getBusinessUnit;

            public DomainServices(
                Func<IEnumerable<StakeholderLogin>> getLogins,
                Func<BusinessUnit> getBusinessUnit,
                Func<Scenario> getScenario)
            {
                Guard.ArgumentNotNull(getLogins, nameof(getLogins));
                Guard.ArgumentNotNull(getScenario, nameof(getScenario));
                Guard.ArgumentNotNull(getBusinessUnit, nameof(getBusinessUnit));

                _getLogins = getLogins;
                _getScenario = getScenario;
                _getBusinessUnit = getBusinessUnit;
            }

            /// <summary>
            ///     Gets the logins associated with the current instance.
            /// </summary>
            public IEnumerable<StakeholderLogin> GetLogins() => _getLogins();

            /// <summary>
            ///     Gets the scenario the stakeholder belongs to.
            /// </summary>
            public Scenario GetScenario() => _getScenario();

            /// <summary>
            ///     Gets the business unit the stakeholder is associated with.
            /// </summary>
            public BusinessUnit GetBusinessUnit() => _getBusinessUnit();
        }
    }
}