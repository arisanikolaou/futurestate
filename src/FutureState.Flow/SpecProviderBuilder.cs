using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FutureState.Specifications;
using Magnum.Reflection;

namespace FutureState.Flow
{
    public class SpecProviderBuilder
    {
        private readonly ISpecProviderFactory _specProviderFactory;

        public SpecProviderBuilder(ISpecProviderFactory specProviderFactory)
        {
            _specProviderFactory = specProviderFactory;
        }

        public void Build(Type type, IList<ValidationRule> rules)
        {
            this.FastInvoke(new[] {type}, "Build", rules);
        }

        public void Build<TEntity>(IList<ValidationRule> rules)
        {
            // should be shared instance across application space
            var provider = _specProviderFactory.Get<TEntity>();

            var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var rule in rules)
            {
                var property = properties.FirstOrDefault(m => m.Name == rule.FieldName);

                if (property != null)
                    provider.Add(m =>
                    {
                        // convert to string
                        var value = Convert.ToString(property.GetValue(m));

                        // run regex rule
                        var regex = new Regex(rule.RegEx);
                        if (regex.IsMatch(value))
                            return SpecResult.Success;

                        // failure
                        return new SpecResult($"Field {property.Name} does not have a valid value: {value}.");
                    }, rule.FieldName, rule.ErrorMessage);
            }
        }
    }
}