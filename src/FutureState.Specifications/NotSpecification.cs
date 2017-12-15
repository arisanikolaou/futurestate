namespace FutureState.Specifications
{
    internal class NotSpecification<TEntity> : ISpecification<TEntity>
    {
        private readonly ISpecification<TEntity> _wrapped;

        internal NotSpecification(ISpecification<TEntity> x)
        {
            _wrapped = x;

            Key = "Not " + x.Key;
            Description = "Not " + x.Description;
        }

        public string Description { get; }

        public string Key { get; }

        public SpecResult Evaluate(TEntity candidate)
        {
            var result = _wrapped.Evaluate(candidate);

            return new SpecResult(!result.IsValid, result.DetailedErrorMessage);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"!({_wrapped})";
        }
    }
}