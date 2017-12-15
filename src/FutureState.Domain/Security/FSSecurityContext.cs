using FutureState.Services;
using System;
using System.Security.Principal;
using System.Threading;

namespace FutureState.Security
{
    /// <summary>
    ///     Represents a configurable security context.
    /// </summary>
    public class FSSecurityContext : IService // mark as service so that its registered in container
    {
        private readonly Func<FSPrinciple> _getPrinciple;

        public FSSecurityContext(Func<FSPrinciple> getActivePrinciple = null)
        {
            _getPrinciple = getActivePrinciple ?? GetCurrentAppPrincipleFromThread;
        }

        public IPrincipal GetCurrentPrinciple()
        {
            return _getPrinciple();
        }

        public FSPrinciple GetCurrentAppPrinciple()
        {
            return _getPrinciple();
        }

        private static FSPrinciple GetCurrentAppPrincipleFromThread()
        {
            var principle = Thread.CurrentPrincipal as FSPrinciple;
            if (principle != null)
            {
                return principle;
            }

            return new FSPrinciple(new FSIdentity("Anonymous", Guid.Empty), new[] { "Everyone" });
        }
    }
}