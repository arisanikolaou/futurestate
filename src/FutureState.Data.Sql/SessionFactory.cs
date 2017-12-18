using System.Data.SqlClient;
using Dapper.Extensions.Linq.Core.Configuration;

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

        public IDapperConfiguration Configuration { get; }

        public string Id { get; set; }

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