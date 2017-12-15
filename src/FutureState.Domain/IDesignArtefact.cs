using FutureState.Data;
using System;

namespace FutureState.Domain
{
    /// <summary>
    ///     An architectural artefact that is part of a given scenario.
    /// </summary>
    public interface IDesignArtefact : IEntityMutableKey<Guid>
    {
        /// <summary>
        ///     Gets the id of the scenario associated with the current instance or null if it is not affiliated with any.
        /// </summary>
        Guid? ScenarioId { get; set; }
    }
}