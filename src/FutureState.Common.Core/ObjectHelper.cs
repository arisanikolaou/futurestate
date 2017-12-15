#region

using System;

#endregion

namespace FutureState
{
    /// <summary>
    /// Provides services to compare object equality and copy values from one object to another.
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Determines whether two objects are equal based on their value type properties
        /// and/or or string properties.
        /// </summary>
        /// <param name="ignoreProperties">
        /// The names of any properties to exclude from the comparison.
        /// </param>
        public static bool AreValuesEqual<T>(T source, T destination, params string[] ignoreProperties)
        {
            if (source == null && null == destination)
            {
                return true; // if both objects are null they are equal
            }

            if (source == null)
            {
                return false;
            }

            if (destination == null)
            {
                return false;
            }

            // don't use source.GetType()
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                // should we ignore a property, such as the id field of an objects?
                var ignoreProperty = false;
                foreach (var propertyToIgnore in ignoreProperties)
                {
                    if (string.Equals(property.Name, propertyToIgnore, StringComparison.OrdinalIgnoreCase))
                    {
                        ignoreProperty = true;
                    }
                }

                if (ignoreProperty)
                {
                    continue;
                }

                // only compare properties with no index parameters
                if (property.GetIndexParameters().Length == 0 && property.GetGetMethod() != null)
                {
                    if (property.PropertyType.IsSubclassOf(typeof(ValueType)))
                    {
                        var a = property.GetValue(source, null) as ValueType;
                        var b = property.GetValue(destination, null) as ValueType;

                        if (!(a == null && b == null))
                        {
                            if (a == null || !a.Equals(b))
                            {
                                return false;
                            }
                        }
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        var a = Convert.ToString(property.GetValue(source, null));
                        var b = Convert.ToString(property.GetValue(destination, null));

                        if (!a.Equals(b, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Assigns an empty string to all properties that are set-able in a given
        /// object that are null.
        /// </summary>
        /// <param name="type">The object to update.</param>
        public static void AssignEmptyStringToNullValues(object type)
        {
            Guard.ArgumentNotNull(type, nameof(type));

            // auto-assign empty string in text fields
            foreach (var property in type.GetType().GetProperties())
            {
                if (property.GetIndexParameters().Length == 0)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        if (Convert.ToString(property.GetValue(type, null)).Length == 0)
                        {
                            if (property.GetSetMethod() != null)
                            {
                                property.SetValue(type, string.Empty, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies a specific set of properties from a source object to a target object.
        /// </summary>
        /// <remarks>
        /// The properties that will be copied will only qualify if they are value type objects
        /// or string objects.
        /// </remarks>
        /// <typeparam name="T">The source type.</typeparam>
        /// <typeparam name="K">The target type.</typeparam>
        /// <param name="destination"> </param>
        /// <param name="propertiesToCopy">The list of properties to copy.</param>
        /// <param name="source"> </param>
        public static void CopySpecificValuesTo<T, K>(T source, K destination, params string[] propertiesToCopy)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentNotNull(destination, nameof(destination));
            Guard.ArgumentNotNull(propertiesToCopy, nameof(propertiesToCopy));

            var sourceProperties = typeof(T).GetProperties();

            var destinationType = destination.GetType();

            foreach (var sourceProperty in sourceProperties)
            {
                // should we ignore a property, such as the id field of an objects?
                var copyProperty = false;
                foreach (var propertyToCopy in propertiesToCopy)
                {
                    if (string.Equals(sourceProperty.Name, propertyToCopy, StringComparison.OrdinalIgnoreCase))
                    {
                        copyProperty = true;
                    }
                }

                if (!copyProperty)
                {
                    continue;
                }

                if (sourceProperty.GetIndexParameters().Length == 0 && sourceProperty.GetGetMethod() != null)
                {
                    if (sourceProperty.PropertyType.IsSubclassOf(typeof(ValueType)) ||
                        sourceProperty.PropertyType == typeof(string))
                    {
                        var destProperty = destinationType.GetProperty(sourceProperty.Name);
                        if (destProperty != null && destProperty.PropertyType == sourceProperty.PropertyType)
                        {
                            var setMethod = destProperty.GetSetMethod();

                            if (setMethod != null)
                            {
                                destProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies value type objects and strings from a source object to a destination object.
        /// </summary>
        /// <remarks>
        /// The properties that will be copied will only qualify if they are value type objects
        /// or string objects.
        /// </remarks>
        /// <typeparam name="T">The type of the source object.</typeparam>
        /// <typeparam name="K">The type of the destination object.</typeparam>
        /// <param name="source">The instance to copy the values from.</param>
        /// <param name="destination">The instance to copy the values to.</param>
        /// <param name="ignoreProperties">An array of property names to not copy at all.</param>
        public static void CopyValuesTo<T, K>(T source, K destination, params string[] ignoreProperties)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentNotNull(destination, nameof(destination));

            var sourceProperties = typeof(T).GetProperties();

            var destinationType = destination.GetType();

            // Type sourceType = source.GetType();
            foreach (var sourceProperty in sourceProperties)
            {
                // should we ignore a property, such as the id field of an objects?
                var ignoreProperty = false;
                foreach (var propertyToIgnore in ignoreProperties)
                {
                    if (string.Equals(sourceProperty.Name, propertyToIgnore, StringComparison.OrdinalIgnoreCase))
                    {
                        ignoreProperty = true;
                    }
                }

                if (ignoreProperty)
                {
                    continue;
                }

                if (sourceProperty.GetIndexParameters().Length == 0 && sourceProperty.GetGetMethod() != null)
                {
                    if (sourceProperty.PropertyType.IsSubclassOf(typeof(ValueType)) ||
                        sourceProperty.PropertyType == typeof(string))
                    {
                        var destProperty = destinationType.GetProperty(sourceProperty.Name);
                        if (destProperty != null && destProperty.PropertyType == sourceProperty.PropertyType)
                        {
                            var setMethod = destProperty.GetSetMethod(true);

                            if (setMethod != null)
                            {
                                destProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                            }
                        }
                    }
                }
            }
        }
    }
}