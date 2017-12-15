using FutureState.Data;
using FutureState.Specifications;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
	/// <summary>
	///     A software/hardware implementation project.
	/// </summary>
	public class Project : FSEntity, IEntityMutableKey<Guid>, IAuditable
	{
        /// <summary>
        ///     Gets the user that last modified the entry.
        /// </summary>
        [NotEmpty("UserName", ErrorMessage = "User name is required.")]
        public string UserName { get; set; }

		/// <summary>
		///     Gets the date the record was last modified in utc.
		/// </summary>
		[NotEmptyDate("Date Last Modified is required.")]
		public DateTime DateLastModified { get; set; }

		/// <summary>
		///     Gets the project's external id.
		/// </summary>
		[StringLength(100)]
		[NotEmpty("ExternalId", ErrorMessage = "External id is required.")]
        public string ExternalId { get; set; }

		/// <summary>
		///     Gets the date the record was created;
		/// </summary>
		[NotEmptyDate("DateCreated", ErrorMessage = "Date Created is required.")]
		public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the start date of the project in Utc.
        /// </summary>
        [NotEmptyDate("StartDate", ErrorMessage = "Project Start Date is required.")]
        public DateTime StartDate { get; set; }

		/// <summary>
		///     Gets the enddate of the project if known.
		/// </summary>
		public DateTime? EndDate { get; set; }

		/// <summary>
		///     Gets the currency to use as a base to express project costs and benefits.
		/// </summary>
		[NotEmpty("Currency", ErrorMessage = "Currency code is required.")]
		public string Currency { get; set; }

		/// <summary>
		///     Gets the business unit the project is associated with if any.
		/// </summary>
		public Guid? BusinessUnitId { get; set; }

		/// <summary>
		///     Create a new instance.
		/// </summary>
		public Project()
		{
			// required by serializers
		}

		/// <summary>
		///     Creates a new instance with a given display name and description.
		/// </summary>
		public Project(string externalId, string displayName, string description, string currency = "USD")
		{
            Guard.ArgumentNotNullOrEmpty(externalId, nameof(externalId));

			this.Id = SeqGuid.Create();

			this.DisplayName = displayName ?? "";
			this.Description = description ?? "";
			this.ExternalId = externalId ?? "";
		    this.Currency = currency;
			this.DateCreated = DateTime.UtcNow;
		    this.DateLastModified = this.DateCreated;
            this.StartDate = this.DateCreated;
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


		public class DomainServices
		{
			public DomainServices(
				Func<BusinessUnit> getBusinessUnit,
				Func<IEnumerable<Scenario>> getScenarios)
			{
				Guard.ArgumentNotNull(getBusinessUnit, nameof(getBusinessUnit));
				Guard.ArgumentNotNull(getScenarios, nameof(getScenarios));

				_scenariosGet = getScenarios;
				_getBusinessUnit = getBusinessUnit;

			}

			readonly Func<IEnumerable<Scenario>> _scenariosGet;
			readonly Func<BusinessUnit> _getBusinessUnit;

			public IEnumerable<Scenario> GetScenarios() => _scenariosGet();
			public BusinessUnit GetBusinessUnit() => _getBusinessUnit();
		}
	}
}