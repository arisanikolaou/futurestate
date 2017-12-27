#region

using System.Collections.Generic;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     Provides specifications (rules) to evaluate the validity state of a given entity or service.
    /// </summary>
    /// <typeparam name="TEntityOrService">The type of entity or service to provide specifications for.</typeparam>
    public interface IProvideSpecifications<in TEntityOrService> : IProvideSpecifications
    {
        /// <summary>
        ///     Gets an enumeration of specifications (rules) for a given entity or service.
        /// </summary>
        IEnumerable<ISpecification<TEntityOrService>> GetSpecifications();
    }

    /// <summary>
    ///     Provides the specifications or rules to use to evaluate the validity state of a given entity.
    /// </summary>
    public interface IProvideSpecifications
    {
    }
}