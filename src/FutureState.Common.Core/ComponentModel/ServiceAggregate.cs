#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState.ComponentModel
{
    /// <summary>
    /// An aggregate of a number of other services in which the ultimate provider of a well known service
    /// is based on a given predicate.
    /// </summary>
    public class ServiceAggregate<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAggregate{TInterface}" /> class.
        /// </summary>
        /// <param name="services">The distinct list of services to compose into the current instance.</param>
        public ServiceAggregate(IEnumerable<T> services)
        {
            Guard.ArgumentNotNull(services, nameof(services));

            //avoid multiple enumeration
            Services = services.ToCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceAggregate{TInterface}" /> class.
        /// </summary>
        public ServiceAggregate(T serviceProviders)
            : this(new[] { serviceProviders })
        {
        }

        protected IEnumerable<T> Services { get; }

        /// <summary>
        /// Demands a service provider that is capable of handling a given service request.
        /// </summary>
        /// <exception cref="NotSupportedException">If not matching service provider is found.</exception>
        public T DemandService(Func<T, bool> predicate)
        {
            var service = SelectService(predicate);
            if (Equals(service, default(T)))
            {
                throw new NotSupportedException("The service or entity type is not supported.");
            }

            return service;
        }

        /// <summary>
        /// Selects a service provider matching a given set of criteria.
        /// </summary>
        protected T SelectService(Func<T, bool> predicate)
        {
            return Services.FirstOrDefault(predicate);
        }
    }
}