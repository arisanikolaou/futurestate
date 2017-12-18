using System;
using System.Reflection;
using Dapper;

namespace FutureState.Data.Sql.Mappings
{
    /// <summary>
    ///     Custom dapper member map.
    /// </summary>
    public class MemberMap : SqlMapper.IMemberMap
    {
        private readonly MemberInfo member;

        public MemberMap(MemberInfo member, string columnName)
        {
            this.member = member;
            ColumnName = columnName;
        }

        public string ColumnName { get; }

        public FieldInfo Field => member as FieldInfo;

        public Type MemberType
        {
            get
            {
                switch (member.MemberType)
                {
                    case MemberTypes.Field: return ((FieldInfo) member).FieldType;
                    case MemberTypes.Property: return ((PropertyInfo) member).PropertyType;
                    default: throw new NotSupportedException($"Member type {member.MemberType} is not supported.");
                }
            }
        }

        public ParameterInfo Parameter => null;
        public PropertyInfo Property => member as PropertyInfo;
    }
}