using FutureState.Data;
using FutureState.Specifications;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     Describes a project scenario. Scenarios are an organizational unit to view an architectural
    ///     design/configuration.
    /// </summary>
    public class Scenario : FSEntity, IEntityMutableKey<Guid>, IAuditable
    {
        /// <summary>
        ///     Gets whether or not the scenario has been initialized and 
        ///     ready to be analyzed.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        ///     Gets the project that owns the current instance.
        /// </summary>
        [NotEmpty("Project Id is required.")]
        public Guid ProjectId { get; set; }

        /// <summary>
        ///     Gets the external reference code for the scenario.
        /// </summary>
        [StringLength(100)]
        [NotEmpty("External Id is required.")]
        public string ExternalId { get; set; }

        /// <summary>
        ///     Gets the date the record was created in utc.
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        ///     Gets the date the entry was last modified.
        /// </summary>
        public DateTime DateLastModified { get; set; }

        /// <summary>
        ///     Gets the name of the user that last modified the entry.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Scenario()
        {
            // required by serializers
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="name">The name of the scenario.</param>
        /// <param name="description">A description of the scenario.</param>
        /// <param name="externalId">External id.</param>
        public Scenario(string name, string description,string externalId)
        {
            Id = SeqGuid.Create(); // todo: replace with combguid or snowflake

            DisplayName = name;
            Description = description;
            ExternalId = externalId ?? "";
            DateCreated = DateTime.UtcNow;
            DateLastModified = DateCreated;
        }

        /// <summary>
        ///     Marks the scenario as initialized. Once a scenario is initialized then
        ///     it can/should not be changed 
        /// </summary>
        public void SetInitialized()
        {
            IsInitialized = true;
        }
    }
}