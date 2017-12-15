using FutureState.Data;
using FutureState.Specifications;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     An identity or login a stakeholder can use to access system resources.
    /// </summary>
    public class StakeholderLogin : IEntityMutableKey<Guid>, IDesignArtefact
    {
        /// <summary>
        ///     Gets the system id for the login.
        /// </summary>
        [Key]
        [NotEmptyGuid("Stakeholder id is required.")]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets the stakeholder id.
        /// </summary>
        [NotEmptyGuid("Stakeholder login is required.")]
        public Guid StakeholderId { get; set; }

        /// <summary>
        ///     Gets or sets the login name. e.g. acme\login
        /// </summary>
        [StringLength(150)]
        [NotEmpty("Name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the login type, e.g. windows, unix, facebook etc.
        /// </summary>
        [StringLength(100)]
        public string LoginType { get; set; }

        /// <summary>
        ///     Gets/sets the login's external identifier.
        /// </summary>
        /// 
        [NotEmpty("External id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the scenario id that this artefact is contained in.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the date the item was created in Utc.
        /// </summary>
        public DateTime DateAdded { get; set; }

        /// <summary>
        ///     Gets the date the login was revoked.
        /// </summary>
        public DateTime? DateExpired { get; set; }

        /// <summary>
        ///     Gets/sets the description.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public StakeholderLogin()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public StakeholderLogin(Stakeholder stakeholder, string externalId, string userName, string description = null, Scenario scenario = null)
        {
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNull(stakeholder, nameof(stakeholder));

            this.Id = SeqGuid.Create();

            this.UserName = userName;
            this.ExternalId = externalId ?? "";
            this.ScenarioId = stakeholder.ScenarioId;
            this.StakeholderId = stakeholder.Id;
            this.Description = description ?? "";
            this.DateAdded = DateTime.UtcNow;
            this.LoginType = "Windows";

            if (scenario != null)
                ScenarioId = scenario.Id;
        }
    }
}