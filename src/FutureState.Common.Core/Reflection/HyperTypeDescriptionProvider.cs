#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;

#endregion

// ReSharper disable once CheckNamespace

namespace Hyper.ComponentModel
{
    public class HyperTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static readonly HashSet<string> _presentTypes = new HashSet<string>();

        private static readonly Dictionary<Type, ICustomTypeDescriptor> descriptors =
            new Dictionary<Type, ICustomTypeDescriptor>();

        public HyperTypeDescriptionProvider()
            : this(typeof(object))
        {
        }

        public HyperTypeDescriptionProvider(Type type)
            : this(TypeDescriptor.GetProvider(type))
        {
        }

        public HyperTypeDescriptionProvider(TypeDescriptionProvider parent)
            : base(parent)
        {
        }

        public static void Add(Type type)
        {
            if (_presentTypes.Contains(type.FullName))
                return;

            _presentTypes.Add(type.FullName);

            var parent = TypeDescriptor.GetProvider(type);
            TypeDescriptor.AddProvider(new HyperTypeDescriptionProvider(parent), type);
        }

        public static void Clear(Type type)
        {
            lock (descriptors)
            {
                descriptors.Remove(type);
            }
        }

        public static void Clear()
        {
            lock (descriptors)
            {
                descriptors.Clear();
            }
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            lock (descriptors)
            {
                ICustomTypeDescriptor descriptor;

                if (!descriptors.TryGetValue(objectType, out descriptor))
                    try
                    {
                        descriptor = BuildDescriptor(objectType);
                    }
                    catch
                    {
                        return base.GetTypeDescriptor(objectType, instance);
                    }

                return descriptor;
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private ICustomTypeDescriptor BuildDescriptor(Type objectType)
        {
            // NOTE: "descriptors" already locked here

            // get the parent descriptor and add to the dictionary so that
            // building the new descriptor will use the base rather than recursing
            var descriptor = base.GetTypeDescriptor(objectType, null);
            descriptors.Add(objectType, descriptor);
            try
            {
                // build a new descriptor from this, and replace the lookup
                descriptor = new HyperTypeDescriptor(descriptor);
                descriptors[objectType] = descriptor;
                return descriptor;
            }
            catch
            {
                // rollback and throw
                // (perhaps because the specific caller lacked permissions;
                // another caller may be successful)
                descriptors.Remove(objectType);
                throw;
            }
        }
    }
}