#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using static System.Enum;

#endregion

namespace FutureState
{
    /// <summary>
    ///     Helper class to retrieve the description of of enumeration objects.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        ///     Gets the Description attribute of an enum
        /// </summary>
        /// <param name="type">The type of the enumeration</param>
        /// <param name="name">The string name of the enumeration value</param>
        /// <returns>The description string</returns>
        public static string GetDescription(Type type, string name)
        {
            var fi = type.GetField(name);
            if (fi == null)
                return string.Empty;

            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : name;
        }

        /// <summary>
        ///     Gets the description attribute of an enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The enum value.</param>
        /// <returns></returns>
        public static string GetDescription<TEnum>(TEnum value)
            where TEnum : struct
        {
            var fi = typeof(TEnum).GetField(value.ToString());
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        /// <summary>
        ///     Gets the Description attribute of an enum value
        /// </summary>
        /// <param name="enumValue">The enum value</param>
        /// <returns>The description string</returns>
        public static string GetDescription(Enum enumValue)
        {
            var type = enumValue.GetType();
            var name = GetName(type, enumValue);
            return GetDescription(type, name);
        }

        /// <summary>
        ///     Extension method to get the value of Description attribute on enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescriptionAttribute(this Enum value)
        {
            var type = value.GetType();

            var memInfo = type.GetMember(value.ToString());

            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs.Length > 0)
                    return ((DescriptionAttribute) attrs[0]).Description;
            }

            return value.ToString();
        }

        /// <summary>
        ///     Gets the value of the description attribute of a given enum value
        /// </summary>
        /// <param name="enumType">The type of enum</param>
        /// <param name="enumField">The enum value</param>
        /// <returns>The description attribute of the enum value or null if it does not exist</returns>
        public static string GetDescriptionAttributeValue(Type enumType, Enum enumField)
        {
            var da =
                (DescriptionAttribute)
                Attribute.GetCustomAttribute(enumType.GetField(enumField.ToString()), typeof(DescriptionAttribute));
            return da == null ? null : da.Description;
        }

        /// <summary>
        ///     Gets the list of descriptions for a enumeration
        /// </summary>
        /// <param name="type">The enum to evaluate</param>
        /// <returns>The list of description</returns>
        public static List<string> GetDescriptions(Type type)
        {
            var result = new List<string>();
            foreach (var name in GetNames(type))
                result.Add(GetDescription(type, name));

            return result;
        }

        /// <summary>
        ///     Gets the Enumeration value for an enumeration given a description.
        ///     Throws Arugument Exception if the description is not found.
        /// </summary>
        /// <param name="type">The enumeration type</param>
        /// <param name="description">The description of the enum value to retrieve</param>
        /// <returns>The corresponding enum value to the description</returns>
        public static Enum GetEnumFromDescription(Type type, string description)
        {
            foreach (var pair in GetEnumValueDict(type))
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

            throw new ArgumentException("Invalid description for type, " + type);
        }

        public static TEnum GetEnumFromDescription<TEnum>(string description)
            where TEnum : struct
        {
            foreach (var pair in GetEnumValueDict<TEnum>())
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

            throw new ArgumentException("Invalid description for type, " + typeof(TEnum));
        }

        public static TEnum GetEnumFromDescription<TEnum>(string description, TEnum _default)
            where TEnum : struct
        {
            foreach (var pair in GetEnumValueDict<TEnum>())
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

            return _default;
        }

        public static TEnum GetEnumFromNameOrDescription<TEnum>(string description, TEnum _default)
            where TEnum : struct
        {
            foreach (var pair in GetEnumValueDict<TEnum>())
            {
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

                if (GetName(typeof(TEnum), pair.Key)
                    .Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;
            }

            return _default;
        }

        public static TEnum? GetEnumFromNameOrDescription<TEnum>(string description, TEnum? _default)
            where TEnum : struct
        {
            foreach (var pair in GetEnumValueDict<TEnum>())
            {
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

                if (GetName(typeof(TEnum), pair.Key)
                    .Equals(description, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;
            }

            return _default;
        }

        public static Enum GetEnumFromNameOrDescription(Type enumType, string nameOrDescription)
        {
            foreach (var pair in GetEnumValueDict(enumType))
            {
                if (pair.Value.Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

                if (GetName(enumType, pair.Key)
                    .Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;
            }

            throw new ArgumentException(
                $"EnumHelper.GetEnumFromNameOrDescription: Invalid name or description ({nameOrDescription}) for type - {enumType.Name} ");
        }

        public static Enum GetEnumFromNameOrDescription(Type enumType, string nameOrDescription, Enum defaultValue)
        {
            foreach (var pair in GetEnumValueDict(enumType))
            {
                if (pair.Value.Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

                if (GetName(enumType, pair.Key)
                    .Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;
            }

            return defaultValue;
        }

        public static Enum GetEnumFromNameOrDescriptionDefaultToFirst(Type enumType, string nameOrDescription)
        {
            var dict = GetEnumValueDict(enumType);
            foreach (var pair in dict)
            {
                if (pair.Value.Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;

                if (GetName(enumType, pair.Key)
                    .Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return pair.Key;
            }

            return dict.First().Key;
        }

        /// <summary>
        ///     Gets a list of TEnumType values from EnumListAttribute
        /// </summary>
        /// <typeparam name="TEnumType"></typeparam>
        /// <typeparam name="TTargetType"></typeparam>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        public static List<TEnumType> GetEnumLimitListValues<TEnumType, TTargetType>(TTargetType targetValue)
        {
            var targetType = typeof(TTargetType);
            var fi = targetType.GetField(targetValue.ToString());
            var attributes = (EnumListAttribute[]) fi.GetCustomAttributes(typeof(EnumListAttribute), false);
            foreach (var attr in attributes)
                if (attr.EnumType == typeof(TEnumType))
                    return attr.Values.OfType<TEnumType>().ToList();

            return GetValues(typeof(TEnumType)).OfType<TEnumType>().ToList();
        }

        public static TEnum GetEnumOrDefault<TEnum>(string value, TEnum @default)
            where TEnum : struct
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim()))
                return @default;

            try
            {
                TEnum val;
                return TryParse(value, true, out val) ? val : @default;
            }
            catch (Exception)
            {
                return @default;
            }
        }

        public static TEnum? GetEnumOrNull<TEnum>(string value, bool ignoreCase = false)
            where TEnum : struct
        {
            TEnum? result = null;

            if (!string.IsNullOrEmpty(value))
            {
                var newValue = value.Trim();

                var names = GetNames(typeof(TEnum));

                foreach (var name in names)
                    if (string.Compare(name, newValue, ignoreCase) == 0)
                    {
                        result = (TEnum) Enum.Parse(typeof(TEnum), name);
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        ///     Gets a dictionary(string name, string description) of an enum type
        /// </summary>
        /// <param name="type">The enumeration to evaluation</param>
        /// <returns>The dictionary result of key = Description, value = EnumName</returns>
        public static Dictionary<string, string> GetEnumStringDict(Type type)
        {
            var result = new Dictionary<string, string>();
            foreach (var name in GetNames(type))
                result.Add(GetDescription(type, name), name);

            return result;
        }

        /// <summary>
        ///     Gets a dictionary(string valueAsString, Enum value) of an enum type
        /// </summary>
        /// <param name="type">The enumeration to evaluation</param>
        /// <returns>The dictionary result of key = value as string, value = Enum</returns>
        public static Dictionary<string, Enum> GetEnumValueAsStringToValueDict(Type type)
        {
            return GetValues(type).Cast<Enum>().ToDictionary(enumValue => enumValue.ToString());
        }

        /// <summary>
        ///     Gets a Collection of tuples(enum value, string description) of an enum type
        /// </summary>
        /// <param name="type">The enumeration to evaluation</param>
        /// <returns>The Tuple result of key = Enum Value, value = Enum Name</returns>
        public static IEnumerable<Tuple<Enum, string>> GetEnumValueDescriptionTuple(Type type)
        {
            return from Enum enumValue in GetValues(type)
                select new Tuple<Enum, string>(enumValue, GetDescription(enumValue));
        }

        public static IEnumerable<Tuple<TEnum, string>> GetEnumValueDescriptionTuple<TEnum>() where TEnum : struct
        {
            return from TEnum enumValue in GetValues(typeof(TEnum)).OfType<TEnum>()
                select new Tuple<TEnum, string>(enumValue, GetDescription(enumValue));
        }

        /// <summary>
        ///     Gets a dictionary(string name, string description) of an enum type
        /// </summary>
        /// <param name="type">The enumeration to evaluation</param>
        /// <returns>The dictionary result of key = Description, value = EnumName</returns>
        public static Dictionary<Enum, string> GetEnumValueDict(Type type)
        {
            var result = new Dictionary<Enum, string>();
            foreach (Enum enumValue in GetValues(type))
                result.Add(enumValue, GetDescription(enumValue));

            return result;
        }

        public static Dictionary<TEnum, string> GetEnumValueDict<TEnum>()
            where TEnum : struct
        {
            var result = new Dictionary<TEnum, string>();
            foreach (TEnum enumValue in GetValues(typeof(TEnum)))
                result.Add(enumValue, GetDescription(enumValue));

            return result;
        }

        public static bool IsNotObsolete(Type enumType, Enum item)
        {
            var da =
                (ObsoleteAttribute)
                Attribute.GetCustomAttribute(enumType.GetField(item.ToString()), typeof(ObsoleteAttribute));
            return da == null;
        }

        public static bool NameOrDescriptionHasEnumValue(Type enumType, string nameOrDescription)
        {
            foreach (var pair in GetEnumValueDict(enumType))
            {
                if (pair.Value.Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (GetName(enumType, pair.Key)
                    .Equals(nameOrDescription, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public static T Parse<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, false);
        }

        /// <summary>
        ///     Get a List of KeyValuePair(Enum, string) of a enumeration type
        /// </summary>
        /// <param name="type">The enumeration to evaluate</param>
        /// <returns>List of (Enum, stringDescription) pairs </returns>
        public static List<KeyValuePair<Enum, string>> ToEnumDescriptionList(Type type)
        {
            var result = new List<KeyValuePair<Enum, string>>();

            foreach (Enum value in GetValues(type))
                result.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));

            return result;
        }

        /// <summary>
        ///     Get a list of KeyValuePair(Enum, stringDescription) of a enumeration type
        /// </summary>
        /// <param name="type">The enumeration to evaluate</param>
        /// <returns>List of (Enum, stringDescription) pairs </returns>
        public static IList ToKeyValuePairList(Type type)
        {
            var result = new ArrayList();

            foreach (Enum value in GetValues(type))
                result.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));

            return result;
        }

        /// <summary>
        ///     Get a list of KeyValuePair of a enumeration type
        /// </summary>
        /// <param name="type">The enumeration to evaluate</param>
        /// <returns>List of (Enum, stringDescription) pairs </returns>
        public static List<KeyValuePair<Enum, string>> ToSortedList(Type type)
        {
            var result = new List<KeyValuePair<Enum, string>>();

            foreach (Enum value in GetValues(type))
                result.Add(new KeyValuePair<Enum, string>(value, GetDescription(value)));

            return result;
        }

        public static bool TryGetEnumFromDescription<TEnum>(string description, out TEnum result) where TEnum : struct
        {
            result = default(TEnum);

            foreach (var pair in GetEnumValueDict<TEnum>())
                if (pair.Value.Equals(description, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = pair.Key;
                    return true;
                }

            return false;
        }

        /// <summary>
        ///     Fetches an attribute of the specified type from Enum.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="value">Enum value.</param>
        /// <returns>A single attribute.</returns>
        public static T TryGetEnumValueAttribute<T>(Enum value) where T : Attribute
        {
            return TryGetEnumValueAttributes<T>(value).SingleOrDefault();
        }

        /// <summary>
        ///     Fetches all attributes of the specified type from Enum.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="value">Enum value.</param>
        /// <returns>All attributes of the specified type.</returns>
        public static IEnumerable<T> TryGetEnumValueAttributes<T>(Enum value) where T : Attribute
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(GetName(type, value));
            if (fieldInfo == null)
                return Enumerable.Empty<T>();

            return (T[]) fieldInfo.GetCustomAttributes(typeof(T), false);
        }

        /// <summary>
        ///     Fetches an attribute of the specified type from Enum.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <typeparam name="TProperty">Type of the attribute property.</typeparam>
        /// <param name="value">Enum value.</param>
        /// <param name="accessor">Attribute property accessor.</param>
        /// <returns>A single attribute.</returns>
        public static TProperty TryGetEnumValueAttributeValue<T, TProperty>(Enum value, Func<T, TProperty> accessor)
            where T : Attribute
        {
            var attribute = TryGetEnumValueAttributes<T>(value).SingleOrDefault();
            return accessor(attribute);
        }
    }

    /// <summary>
    ///     Attribute allows to specify set of allowable Enum values
    ///     note. Can also be used together with UI attributes to limit the enum values list
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class EnumListAttribute : Attribute
    {
        public EnumListAttribute(Type tp, params object[] acceptableValues)
        {
            if (tp == null)
                throw new ArgumentNullException("Type argument should not be null", nameof(tp));

            if (!tp.IsEnum)
                throw new InvalidOperationException(
                    "The EnumListAttribute attribute should be applied to Enum type only.");

            EnumType = tp;
            Values = acceptableValues;
        }

        public Type EnumType { get; }

        public object[] Values { get; }
    }

    /// <summary>
    ///     The converter works together with EnumListAttribute.
    ///     GetStandardValues returns the list of enum values specifyed in EnumListAttribute
    /// </summary>
    public class LimitEnumConverter : EnumConverter
    {
        private StandardValuesCollection _enumVals;

        public LimitEnumConverter(Type type)
            : base(type)
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (_enumVals == null)
            {
                // first time the list for the type is requested
                foreach (var attr in context.PropertyDescriptor.Attributes)
                {
                    var enList = attr as EnumListAttribute;
                    if (enList != null)
                    {
                        // Get possible values from the attribute instance
                        _enumVals = new StandardValuesCollection(enList.Values);
                        return _enumVals;
                    }
                }

                // No Enum List - return all Enum Values
                if (context.PropertyDescriptor.PropertyType.IsEnum)
                    _enumVals = new StandardValuesCollection(GetValues(context.PropertyDescriptor.PropertyType));
                else
                    _enumVals = new StandardValuesCollection(null);
            }

            return _enumVals;
        }
    }

    /// <summary>
    ///     Converts description to enum value.
    ///     Also limits the possible values (derived from EnumListAttribute)
    ///     if value limit is not needed - should be derived directly from EnumConverter
    /// </summary>
    public class EnumDescriptionToEnumConverter : LimitEnumConverter
    {
        public EnumDescriptionToEnumConverter(Type type)
            : base(type)
        {
        }

        public static object GetEnumValueFromDescription(Type value, string description)
        {
            var fis = value.GetFields();
            foreach (var fi in fis)
            {
                var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                    if (attributes[0].Description == description)
                        return fi.GetValue(fi.Name);

                if (fi.Name == description)
                    return fi.GetValue(fi.Name);
            }

            return description;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
                return GetEnumValueFromDescription(context.PropertyDescriptor.PropertyType, (string) value);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            if (value is Enum && destinationType == typeof(string))
                return EnumHelper.GetDescription(value.GetType(), value.ToString());

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}