#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#endregion

namespace FutureState
{
    // todo: unit test

    /// <summary>
    ///     Provides ways of parsing enums from text fields in an optimized way over Enum.Parse
    /// </summary>
    /// <typeparam name="TEnum">The enumeration to parse.</typeparam>
    public static class EnumHelperEx<TEnum>
        where TEnum : struct
    {
        private static readonly Dictionary<string, TEnum> DbOfEnumDescriptionsToValues;

        private static readonly Dictionary<string, TEnum> DbOfEnumNamesToValues;

        static EnumHelperEx()
        {
            DbOfEnumDescriptionsToValues = new Dictionary<string, TEnum>();
            DbOfEnumNamesToValues = new Dictionary<string, TEnum>();

            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                DbOfEnumDescriptionsToValues.Add(GetDescription(enumValue.ToString()).ToLower(), enumValue);

                DbOfEnumNamesToValues.Add(enumValue.ToString().ToLower(), enumValue);
            }
        }

        /// <summary>
        ///     Gets the enum equivalent from a string value and/or throws an exception of the enum value does not exist.
        /// </summary>
        /// <remarks>
        ///     The supplied value is converted on a case insensitive way and is trimmed.
        /// </remarks>
        public static TEnum GetEnum(string value)
        {
            if (value == null)
                throw new NotSupportedException(
                    "Null can't be converted into a {0} type.".Params(typeof(TEnum).FullName));

            value = value.Trim().ToLower();

            if (DbOfEnumNamesToValues.ContainsKey(value))
                return DbOfEnumNamesToValues[value];

            throw new NotSupportedException("The value {0} can't be converted into a {1} type.".Params(value,
                typeof(TEnum).FullName));
        }

        public static TEnum GetEnumFromNameOrDescription(string description, TEnum defaultValue)
        {
            description = description.ToLower();

            if (DbOfEnumDescriptionsToValues.ContainsKey(description))
                return DbOfEnumDescriptionsToValues[description];

            if (DbOfEnumNamesToValues.ContainsKey(description))
                return DbOfEnumNamesToValues[description];

            return defaultValue;
        }

        public static TEnum? GetEnumFromNameOrDescriptionOrNull(string description, TEnum? defaultValue)
        {
            description = description.ToLower();

            if (DbOfEnumDescriptionsToValues.ContainsKey(description))
                return DbOfEnumDescriptionsToValues[description];

            if (DbOfEnumNamesToValues.ContainsKey(description))
                return DbOfEnumNamesToValues[description];

            return defaultValue;
        }

        /// <summary>
        ///     Gets the enum equivalent from a string value and/or returns the default value.
        /// </summary>
        /// <remarks>
        ///     The supplied value is converted on a case insensitive way and is trimmed.
        /// </remarks>
        /// <param name="value">The value to convert.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TEnum GetEnumOrDefault(string value, TEnum defaultValue = default(TEnum))
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            value = value.Trim().ToLower();

            if (string.IsNullOrEmpty(value))
                return defaultValue;

            if (DbOfEnumNamesToValues.ContainsKey(value))
                return DbOfEnumNamesToValues[value];

            return defaultValue;
        }

        /// <summary>
        ///     Gets the Description attribute of an enum
        /// </summary>
        /// <param name="name">The string name of the enumeration value</param>
        /// <returns>The description string</returns>
        private static string GetDescription(string name)
        {
            var type = typeof(TEnum);

            var fi = type.GetField(name);
            if (fi == null)
                return string.Empty;

            var attributes = (DescriptionAttribute[])
                fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes.First().Description : name;
        }
    }
}