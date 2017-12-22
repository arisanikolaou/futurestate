namespace FutureState.Specifications
{
    /// <summary>
    ///     A rule or constraint to evaluate against a given domain object.
    /// </summary>
    /// <typeparam name="TEntity">The domain object type to evaluate.</typeparam>
    public interface ISpecification<in TEntity>
    {
        /// <summary>
        ///     Gets a description of the specification.
        /// </summary>
        string Description { get; }

        /// <summary>
        ///     Gets a key or code to identifier the specification.
        /// </summary>
        string Key { get; }

        /// <summary>
        ///     Gets whether the specification is met by the given entity.
        /// </summary>
        SpecResult Evaluate(TEntity entity);
    }
}