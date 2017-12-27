using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;

namespace FutureState.Data.Sql.Mappings
{
    // custom type map for dapper

    public class CustomTypeMap<TEntity> : SqlMapper.ITypeMap
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Type _undelyingType;

        private readonly SqlMapper.ITypeMap _innerTypeMap;
        private readonly Dictionary<string, SqlMapper.IMemberMap> _properties;

        static CustomTypeMap()
        {
            _undelyingType = typeof(TEntity);
        }

        public CustomTypeMap(SqlMapper.ITypeMap innerTypeMap)
        {
            _innerTypeMap = innerTypeMap;
            _properties = new Dictionary<string, SqlMapper.IMemberMap>(StringComparer.OrdinalIgnoreCase);
        }

        public Type UndelyingType => _undelyingType;

        public ConstructorInfo FindConstructor(string[] names, Type[] types)
        {
            return _innerTypeMap.FindConstructor(names, types);
        }

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string memberName)
        {
            return _innerTypeMap.GetConstructorParameter(constructor, memberName);
        }

        public SqlMapper.IMemberMap GetMember(string memberName)
        {
            SqlMapper.IMemberMap map;
            if (!_properties.TryGetValue(memberName, out map))
                map = _innerTypeMap.GetMember(memberName);
            return map;
        }

        public ConstructorInfo FindExplicitConstructor()
        {
            return _innerTypeMap.FindExplicitConstructor();
        }

        public void Map(string memberName, string columnName)
        {
            _properties[memberName] = new MemberMap(_undelyingType.GetMember(memberName).Single(), columnName);
        }
    }
}