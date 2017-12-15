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
        private readonly Dictionary<string, SqlMapper.IMemberMap> _properties;

        public Type UndelyingType => _undelyingType;
        // ReSharper disable once StaticMemberInGenericType
        static readonly Type _undelyingType;
        private readonly SqlMapper.ITypeMap _innerTypeMap;

        static CustomTypeMap()
        {
            _undelyingType = typeof(TEntity);
        }

        public CustomTypeMap(SqlMapper.ITypeMap innerTypeMap)
        {
            this._innerTypeMap = innerTypeMap;
            _properties = new Dictionary<string, SqlMapper.IMemberMap>(StringComparer.OrdinalIgnoreCase);
        }

        public void Map(string memberName, string columnName)
        {
            _properties[memberName] = new MemberMap(_undelyingType.GetMember(memberName).Single(), columnName);
        }

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
            { // you might want to return null if you prefer not to fallback to the
              // default implementation
                map = _innerTypeMap.GetMember(memberName);
            }
            return map;
        }

        public ConstructorInfo FindExplicitConstructor()
        {
            return _innerTypeMap.FindExplicitConstructor();
        }
    }
}
