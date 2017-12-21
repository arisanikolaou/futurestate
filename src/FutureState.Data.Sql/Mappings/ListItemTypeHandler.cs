using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace FutureState.Data.Sql.Mappings
{
    /// <summary>
    ///     Custom dapper type hanlder to support collection of poco items.
    /// </summary>
    /// <typeparam name="TItemType">
    /// The type of item to store in the collection.
    /// </typeparam>
    public class JsonListTypeHandler<TItemType> : SqlMapper.TypeHandler<List<TItemType>>
    {
        public override void SetValue(IDbDataParameter parameter, List<TItemType> value)
        {
            parameter.Value = JsonConvert.SerializeObject(value);
        }

        public override List<TItemType> Parse(object value)
        {
            return JsonConvert.DeserializeObject<List<TItemType>>(Convert.ToString(value));
        }
    }
}