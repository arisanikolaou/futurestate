namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class MySqlDatabaseOptions : SqlDatabaseOptions
    {
        public MySqlDatabaseOptions()
        {
            StartDelimiter = EndDelimiter = "`";
        }
    }
}