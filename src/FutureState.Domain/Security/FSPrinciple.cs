using System;
using System.Linq;
using System.Security.Principal;

namespace FutureState.Security
{
    public class FSPrinciple : IPrincipal
    {
        private readonly string[] _roles;

        public bool IsInRole(string role)
        {
            return _roles.Any(m => string.Equals(m, role, StringComparison.OrdinalIgnoreCase));
        }

        public IIdentity Identity { get; }

        public FSIdentity FSIdentity { get; }

        public FSPrinciple(FSIdentity identity, string[] roles = null)
        {
            Guard.ArgumentNotNull(identity, nameof(identity));

            Identity = identity;
            FSIdentity = identity;

            // roles
            _roles = roles ?? new[] { "Everyone" };
        }
    }
}