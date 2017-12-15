// ReSharper disable once CheckNamespace (the namespace is intentionally not in sync with the file location)

using System;

namespace Dapper.FastCrud
{
    /// <summary>
    ///     Denotes that a column has a default value assigned by the database.
    ///     Properties marked with this attributes will be ignored on INSERT but refreshed from the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DatabaseGeneratedDefaultValueAttribute : Attribute
    {
    }
}