using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using Dapper.Extensions.Linq.Core.Configuration;
using FutureState.Data.Sql.Mappings;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     Creates sessions.
    /// </summary>
    public class SessionFactory : ISessionFactory
    {
        readonly string _conString;

        public string Id { get; set; }
        public IDapperConfiguration Configuration { get; }

        /// <summary>
        ///     Creates a new session factory.
        /// </summary>
        /// <param name="conString">The connection string to the sql server database.</param>
        /// <param name="configuration">The dapper configuration to use.</param>
        public SessionFactory(string conString, IDapperConfiguration configuration)
        {
            Guard.ArgumentNotNull(configuration, nameof(configuration));
            Guard.ArgumentNotNullOrEmptyOrWhiteSpace(conString, nameof(conString));

            _conString = conString;
            Configuration = configuration;
        }

        /// <summary>
        ///     Opens/creates a new session.
        /// </summary>
        /// <returns>A new session.</returns>
        public ISession OpenSession()
        {
            var con = new SqlConnection(_conString);

            return new Session(con, Configuration);
        }
    }
}
