using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FutureState.Specifications
{
    /// <summary>
    ///     Builds a list of specifications for a given entity based on the <see cref="ValidationAttribute" />
    ///     declared in the entity's public properties.
    /// </summary>
    public class DataAnnotationsSpecProvider<T> : IProvideSpecifications<T>
    {
// ReSharper disable StaticFieldInGenericType
        // ReSharper disable once InconsistentNaming
        private static readonly PropertyInfo[] _properties;

        private IEnumerable<ISpecification<T>> _specs;

        static DataAnnotationsSpecProvider()
        {
            // interface data annotations should be considered - they won't be returned with a given flatten hierarchy call
            var types = new[] {typeof(T)}.Concat(typeof(T).GetInterfaces());

            var flattenedProperties = new List<PropertyInfo>();
            types.Each(
                n =>
                    flattenedProperties.AddRange(
                        n.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                            .Where(
                                m =>
                                    m.GetIndexParameters().Length == 0
                                    && m.GetCustomAttributes(typeof(ValidationAttribute), true).Any()
                                    && flattenedProperties.All(o => o.Name != m.Name))));
            // don't duplicate properties by name

            _properties = flattenedProperties.ToArray();
        }

        /// <summary>
        ///     Validates the state of an object based on its property annotations.
        /// </summary>
        public IEnumerable<ISpecification<T>> GetSpecifications()
        {
            // lazy load
            if (_specs != null)
                return _specs;

            // specs won't and shouldn't changed within the application's lifetime
            return _specs = GetSpecs();
        }

        /// <summary>
        ///     Gets the properties of the given instance type that can be tested.
        /// </summary>
        protected virtual PropertyInfo[] GetProperties()
        {
            return _properties;
        }

        private IEnumerable<ISpecification<T>> GetSpecs()
        {
            foreach (var property in GetProperties())
            {
                var customAttributes = property.GetCustomAttributes(typeof(ValidationAttribute), true)
                    .Where(m => m is ValidationAttribute).ToList();

                foreach (ValidationAttribute attribute in customAttributes)
                {
                    var spec = CreateSpec(property, attribute);
                    if (spec != null)
                        yield return spec;
                }
            }

            foreach (var spec in GetCustomSpecs())
                yield return spec;
        }

        /// <summary>
        ///     When derived gets a list of specs appropriate to validate a given object.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ISpecification<T>> GetCustomSpecs()
        {
            yield break;
        }

        private ISpecification<T> CreateSpec(PropertyInfo property, ValidationAttribute attribute)
        {
            if (attribute is StringLengthAttribute)
            {
                var specification = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        var stringLengthAttribute = attribute as StringLengthAttribute;

                        var value = Convert.ToString(property.GetValue(domainObject, null));
                        if (value.Length > stringLengthAttribute.MaximumLength)
                        {
                            var msg =
                                "The length of the '{0}' value is greater than {1} characters.".Params(
                                    property.Name,
                                    stringLengthAttribute.MaximumLength);
                            detailedErrorMessage.AppendLine(msg);

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        if (value.Length < stringLengthAttribute.MinimumLength)
                        {
                            var msg = "The length of the '{0}' value is less than {1} characters.".Params(
                                property.Name,
                                stringLengthAttribute.MinimumLength);
                            detailedErrorMessage.AppendLine(msg);

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    "'{0}' is either too long or too short.".Params(property.Name)); // description

                return specification;
            }

            var notEmptyAttribute = attribute as NotEmptyAttribute;
            if (notEmptyAttribute != null)
            {
                var property1 = property;

                var notEmptySpec = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        // null generics will be converted to an empty string
                        var value = property1.GetValue(domainObject, null);

                        var validationResult =
                            notEmptyAttribute.GetValidationResult(value, new ValidationContext(domainObject));

                        if (validationResult != ValidationResult.Success)
                        {
                            // pull from resource file
                            if (!notEmptyAttribute.ErrorMessage.Exists())
                            {
                                var msg = "'{0}' cannot be null or empty.".Params(property1.Name);

                                detailedErrorMessage.AppendLine(msg);
                            }
                            else
                            {
                                detailedErrorMessage.Append(notEmptyAttribute.ErrorMessage);
                            }

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    $"'{property1.Name}' cannot be null or empty.");

                return notEmptySpec;
            }

            if (attribute is RequiredAttribute)
            {
                var requiredAttribute = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        // null generics will be converted to an empty string
                        var value = property.GetValue(domainObject, null);

                        if (value == null)
                        {
                            // todo: pull from resource file
                            var msg = "'{0}' is a required field.".Params(property.Name);
                            detailedErrorMessage.AppendLine(msg);

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    "'{0}' is a required field.".Params(property.Name));

                return requiredAttribute;
            }

            if (attribute is RegularExpressionAttribute)
            {
                Attribute attribute1 = attribute;
                var property1 = property;

                var regexSpec = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        var expressionAttribute = attribute1 as RegularExpressionAttribute;

                        // null generics will be converted to an empty string
                        var value = property1.GetValue(domainObject, null);

                        if (!expressionAttribute.IsValid(value))
                        {
                            // pull from resource file
                            if (!expressionAttribute.ErrorMessage.Exists())
                            {
                                var msg =
                                    "'{0}' does not meet the required pattern or range. The current value is {1}."
                                        .Params(property1.Name, value);

                                detailedErrorMessage.AppendLine(msg);
                            }
                            else
                            {
                                detailedErrorMessage.Append(expressionAttribute.ErrorMessage);
                            }

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    @"One or more values do not meet the required validation expressions.");

                return regexSpec;
            }

            if (attribute is RangeAttribute)
            {
                Attribute attribute1 = attribute;
                var property1 = property;

                var regexSpec = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        var expressionAttribute = attribute1 as RangeAttribute;

                        // null generics will be converted to an empty string
                        var value = property1.GetValue(domainObject, null);

                        if (!expressionAttribute.IsValid(value))
                        {
                            // pull from resource file
                            if (!expressionAttribute.ErrorMessage.Exists())
                            {
                                var msg =
                                    "'{0}' does not meet the required range pattern. The minimum allowed is {1}  and the maximum {2}. The current value is {3}."
                                        .Params(
                                            property1.Name,
                                            expressionAttribute.Minimum,
                                            expressionAttribute.Maximum,
                                            value);

                                detailedErrorMessage.AppendLine(msg);
                            }
                            else
                            {
                                var formattedErrorMessage = expressionAttribute.FormatErrorMessage(property1.Name);
                                detailedErrorMessage.Append(formattedErrorMessage);
                            }

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    @"One or more values do not meet the required validation expressions.");

                return regexSpec;
            }

            if (attribute != null)
            {
                var regexSpec = new Specification<T>(
                    domainObject =>
                    {
                        var detailedErrorMessage = new StringBuilder();

                        var validationAttribute = attribute;

                        // null generics will be converted to an empty string
                        var value = property.GetValue(domainObject, null);

                        if (!validationAttribute.IsValid(value))
                        {
                            // pull from resource file
                            if (!validationAttribute.ErrorMessage.Exists())
                            {
                                var msg =
                                    "'{0}' does not meet its requirements. The current value is {1}.".Params(
                                        property.Name,
                                        value);
                                detailedErrorMessage.AppendLine(msg);
                            }
                            else
                            {
                                var formattedErrorMessage = validationAttribute.FormatErrorMessage(property.Name);
                                detailedErrorMessage.Append(formattedErrorMessage);
                            }

                            return new SpecResult(false, detailedErrorMessage.ToString());
                        }

                        return SpecResult.Success;
                    },
                    property.Name,
                    @"One or more values do not meet the required validation expressions.");

                return regexSpec;
            }

            return null;
        }

        // properties will never change in the lifetime of the application a.n.a
    }
}