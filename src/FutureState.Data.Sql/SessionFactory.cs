﻿using Dapper.Extensions.Linq.Core.Configuration;
using System.Data.SqlClient;

namespace FutureState.Data.Sql
{
    /// <summary>
    ///     Creates sessions.
    /// </summary>
    public class SessionFactory : ISessionFactory
    {
        private readonly string _conString;

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
        ///     Gets the active dapper configuration used by the session factory.
        /// </summary>
        public IDapperConfiguration Configuration { get; }

        /// <summary>
        ///     Gets/sets an optional identifier of the connection.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Opens/creates a new session.
        /// </summary>
        /// <returns>A new session.</returns>
        public ISession Create()
        {
            var con = new SqlConnection(_conString);

            return new Session(con, Configuration);
        }
    }
}