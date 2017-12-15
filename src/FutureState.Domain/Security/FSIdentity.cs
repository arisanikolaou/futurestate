using System;
using System.Security.Principal;

namespace FutureState.Security
{
    public class FSIdentity : IIdentity
    {
        public string Name { get; }

        public string AuthenticationType { get; }

        public bool IsAuthenticated { get; set; }

        public FSIdentity(string name, Guid userId)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            Name = name;
            UserId = userId;
            AuthenticationType = "FutureState";
        }

        public Guid UserId { get; }

        public bool IsAnonymous => UserId == Guid.Empty;
    }
}