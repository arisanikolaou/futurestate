using FutureState.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace FutureState.Domain
{
    /// <summary>
    ///     A record of the dependency betewen a software model as well as software model
    ///     that it depends on.
    /// </summary>
    public class SoftwareModelDependency : IEntityMutableKey<Guid>, IDesignArtefact
    {
        /// <summary>
        ///     Get the entity's key.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets the scenario id the dependency belongs to or null if it belongs
        ///     to the default scenario.
        /// </summary>
        public Guid? ScenarioId { get; set; }

        /// <summary>
        ///     Gets the software model id that has the dependency.
        /// </summary>
        public Guid SoftwareModelId { get; set; }

        /// <summary>
        ///     Gets the software model id that is depended on.
        /// </summary>
        public Guid SoftwareModelDependencyId { get; set; }

        /// <summary>
        ///     Gets the description of the dependency.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public SoftwareModelDependency()
        {
            // required by serializer
        }

        /// <summary>
        ///     Creates a new instance recording the dependency to a given
        /// </summary>
        /// <param name="dependendent">The software model that owns the dependency.</param>
        /// <param name="model">The software model that is depended on.</param>
        public SoftwareModelDependency(SoftwareModel dependendent, SoftwareModel model, string description = null)
        {
            Guard.ArgumentNotNull(dependendent, nameof(dependendent));
            Guard.ArgumentNotNull(model, nameof(model));

            if (dependendent.ScenarioId != model.ScenarioId)
                throw new InvalidOperationException("Models belong to different scenarios.");

            if (dependendent.Id == model.Id)
                throw new ArgumentOutOfRangeException(nameof(dependendent), "Software models cannot depend on themselves.");

            this.Id = SeqGuid.Create();
            // register to same scenario
            this.ScenarioId = dependendent.ScenarioId;

            this.SoftwareModelId = dependendent.Id;
            this.SoftwareModelDependencyId = model.Id;
            this.Description = description ?? "";
        }
    }
}