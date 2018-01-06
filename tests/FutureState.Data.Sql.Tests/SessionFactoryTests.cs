using Dapper.Extensions.Linq.Core.Configuration;
using Xunit;

namespace FutureState.Data.Sql.Tests
{
    public class SessionFactoryTests
    {
        public static string LocalDbServerName { get; set; } = @"(localdb)\MSSQLLocalDB";

        // connection string to local db
        public static string ConnectionString =>
            $@"Data Source={LocalDbServerName};Initial Catalog=master;Integrated Security=True";

        [Fact]
        public void CanOpenAndCloseSession()
        {
            var subject = new SessionFactory(ConnectionString, DapperConfiguration.Use());

            using (
                ISession session = subject.Create())
            {
                ITransaction tran = session.BeginTran();

                tran.Commit();
            }
        }
    }
}
