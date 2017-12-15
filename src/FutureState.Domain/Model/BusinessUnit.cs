using FutureState.Data;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Business units own asset, stakeholders and networks.
    /// </summary>
    public class BusinessUnit : FSEntity, IEntityMutableKey<Guid>
    {
        /// <summary>
        ///     Gets the business unit code.
        /// </summary>
        [StringLength(100)]
        [NotEmpty("ExternalId", ErrorMessage = "External id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the fiscal start of the year.
        /// </summary>
        [NotEmptyDate("Date Active")]
        public DateTime DateActive { get; set; }

        /// <summary>
        ///     Custom attributes.
        /// </summary>
        public List<Item> Attributes { get; set; }

        /// <summary>
        ///     Gets the date the business unit retired. If null it is still active.
        /// </summary>
        public DateTime? DateRetired { get; set; }

        /// <summary>
        ///     Gets the currency that is used.
        /// </summary>
        [StringLength(3)]
        [NotEmpty("Currency", ErrorMessage = "Default business unit currency is required.")]
        public string Currency { get; set; }

        /// <summary>
        ///     Gets the business unit's containing unit.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        ///     Gets the user that last updated the entry.
        /// </summary>
        [NotEmpty("UserName", ErrorMessage = "User name is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets the date the entry was last modified in utc.
        /// </summary>
        [NotEmptyDate("Date last modified cannot be an empty date.")]
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Creates a new business unit.
        /// </summary>
        public BusinessUnit()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance with a given display name and description.
        /// </summary>
        public BusinessUnit(string externalId, string name, string description, string currency = "USD")
        {
            Guard.ArgumentNotNullOrEmpty(externalId, nameof(externalId));

            Id = SeqGuid.Create();

            ExternalId = externalId;
            Currency = currency ?? "";
            DateActive = DateTime.UtcNow;
            DateLastModified = DateActive;
            Description = description;
            DisplayName = name;
        }
    }
}