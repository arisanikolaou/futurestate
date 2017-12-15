namespace FutureState.Specifications
{
    public class AlwaysTrueSpecification<T> : ISpecification<T>
    {
        /// <summary>
        /// Gets a description of the specification.
        /// </summary>
        public string Description => "Always true";

        /// <summary>
        /// Gets a key or code to identifier the specification.
        /// </summary>
        public string Key => bool.TrueString;

        /// <summary>
        /// Gets whether the specification is met by the given entity.
        /// </summary>
        public SpecResult Evaluate(T entity)
        {
            return new SpecResult(true);
        }
    }
}