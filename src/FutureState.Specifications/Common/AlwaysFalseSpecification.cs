namespace FutureState.Specifications
{
    public class AlwaysFalseSpecification<T> : ISpecification<T>
    {
        /// <summary>
        ///     Gets a description of the specification.
        /// </summary>
        public string Description => "Always false";

        /// <summary>
        ///     Gets a key or code to identifier the specification.
        /// </summary>
        public string Key => bool.FalseString;

        /// <summary>
        ///     Gets whether the specification is met by the given entity.
        /// </summary>
        public SpecResult Evaluate(T entity)
        {
            return new SpecResult(false, "Always false");
        }
    }
}