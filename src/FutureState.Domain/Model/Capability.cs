using FutureState.Data;
using FutureState.Specifications;
using System;

namespace FutureState.Domain
{
    /// <summary>
    ///     A business capability provided by an design artefact.
    /// </summary>
    /// <remarks>
    ///     Capabilities provide business value.
    /// </remarks>
    public class Capability : FSEntity, IEntityMutableKey<Guid>, IDesignArtefact, IAuditable
    {
        /// <summary>
        ///     Gets/sets the user that last modified the record.
        /// </summary>
        [NotEmpty("User name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets/sets the date the user was last modified.
        /// </summary>
        [NotEmptyDate("Date last modified is required.")]
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the architectural scenario this artifact belongs to.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the one time value for the capability for the business.
        /// </summary>
        public double? BusinessValue { get; set; }

        /// <summary>
        ///     Gets the annaul perpetual business value to be expected for the business.
        /// </summary>
        public double? AnnualBusinessValue { get; set; }

        /// <summary>
        ///     Gets the id of the enterprise model this capability is associated with.
        /// </summary>
        [NotEmptyGuid("Software Model that contains the capability.")]
        public Guid SoftwareModelId { get; set; }

        /// <summary>
        ///     Gets/sets the business unit that owns the capability.
        /// </summary>
        public Guid? BusinessUnitId { get; set; }

        /// <summary>
        ///     Gets the date, in utc, this entity was created.
        /// </summary>
        [NotEmptyDate("Date created is required.")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the external identifier for the capability.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Capability()
        {
            // required by the serializer
        }

        /// <summary>
        ///     Creates a new instance associated with a given architectural artefact, display name and/or description.
        /// </summary>
        public Capability(SoftwareModel softwareModel, string externalId, string displayName, string description = null)
        {
            Guard.ArgumentNotNull(softwareModel, nameof(softwareModel));
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(displayName, nameof(displayName));

            this.DisplayName = displayName;
            this.Description = description ?? "";
            this.DateCreated = DateTime.UtcNow;
            this.ExternalId = externalId ?? "";
            this.ScenarioId = softwareModel.ScenarioId;
            this.SoftwareModelId = softwareModel.Id;
            this.DateLastModified = DateCreated;
            this.UserName = "";

            this.Id = SeqGuid.Create();
        }
    }
}