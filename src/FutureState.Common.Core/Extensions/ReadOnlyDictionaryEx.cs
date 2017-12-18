#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace FutureState
{
    // todo: unit tests
    public static class ReadOnlyDictionaryEx
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Props =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        ///     Converts an object's state to a read only dictionary based on the values provided
        ///     in their public properties.
        /// </summary>
        public static ReadOnlyDictionary<string, object> ToReadOnlyDict<TValue>(this TValue value)
        {
            if (value == null)
                return new ReadOnlyDictionary<string, object>();

            PropertyInfo[] props;

            // get from internal cache as this call can be expensive
            var type = typeof(TValue);
            if (Props.ContainsKey(type))
            {
                props = Props[type];
            }
            else
            {
                props =
                    type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(m => !m.GetIndexParameters().Any())
                        .ToArray();
                Props.TryAdd(type, props);
            }

            var hashSet = new Dictionary<string, object>();

            foreach (var propertyInfo in props)
            {
                var propertyValue = propertyInfo.GetValue(value, null);

                if (propertyInfo.PropertyType.IsValueType)
                {
                    hashSet.Add(propertyInfo.Name, propertyValue);
                }
                else if (typeof(ICloneable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var clonedInstance = propertyValue as ICloneable;

                    // prefer cloneable implementation
                    if (clonedInstance != null)
                        propertyValue = clonedInstance.Clone();

                    // otherwise null
                    hashSet.Add(propertyInfo.Name, propertyValue);
                }
                else
                {
                    hashSet.Add(propertyInfo.Name, propertyValue);
                }
            }

            return new ReadOnlyDictionary<string, object>(hashSet);
        }
    }
}