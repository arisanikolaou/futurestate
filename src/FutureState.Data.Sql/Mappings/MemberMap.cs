using Dapper;
using System;
using System.Reflection;

namespace FutureState.Data.Sql.Mappings
{
    /// <summary>
    ///     Custom dapper member map.
    /// </summary>
    public class MemberMap : SqlMapper.IMemberMap
    {
        private readonly MemberInfo _member;

        public MemberMap(MemberInfo member, string columnName)
        {
            this._member = member;
            ColumnName = columnName;
        }

        public string ColumnName { get; }

        public FieldInfo Field => _member as FieldInfo;

        public Type MemberType
        {
            get
            {
                switch (_member.MemberType)
                {
                    case MemberTypes.Field: return ((FieldInfo)_member).FieldType;
                    case MemberTypes.Property: return ((PropertyInfo)_member).PropertyType;
                    default: throw new NotSupportedException($"Member type {_member.MemberType} is not supported.");
                }
            }
        }

        public ParameterInfo Parameter => null;
        public PropertyInfo Property => _member as PropertyInfo;
    }
}