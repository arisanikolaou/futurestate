namespace FutureState.Specifications
{
    internal class OrSpecification<TEntity> : ISpecification<TEntity>
    {
        private readonly ISpecification<TEntity> _spec1;

        private readonly ISpecification<TEntity> _spec2;

        internal OrSpecification(ISpecification<TEntity> s1, ISpecification<TEntity> s2)
        {
            _spec1 = s1;
            _spec2 = s2;

            Key = s1.Key + " Or " + s2.Key;

            Description = @"Or Specification";
        }

        public string Description { get; }

        public string Key { get; }

        public SpecResult Evaluate(TEntity candidate)
        {
            var result1 = _spec1.Evaluate(candidate);
            var result2 = _spec2.Evaluate(candidate);

            return new SpecResult(
                result1.IsValid || result2.IsValid,
                $"{result1.DetailedErrorMessage} Or {result2.DetailedErrorMessage}");
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"({_spec1} | {_spec2})";
        }
    }
}