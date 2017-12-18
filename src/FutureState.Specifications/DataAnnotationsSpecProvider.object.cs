#region

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

#endregion

namespace FutureState.Specifications
{
    // an - generic object validation
    public class DataAnnotationsSpecProvider : DataAnnotationsSpecProvider<object>
    {
        // properties will never change in the lifetime of the application a.n.a
        private readonly PropertyInfo[] _properties;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataAnnotationsSpecProvider(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            // get a list of properties to evaluate and ignore indexed parameters
            _properties =
                type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                    .Where(
                        m =>
                            m.GetIndexParameters().Length == 0 &&
                            m.GetCustomAttributes(typeof(ValidationAttribute), true).Any()).ToArray();
        }

        protected override PropertyInfo[] GetProperties()
        {
            return _properties;
        }
    }
}