namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class PostreSqlDatabaseOptions : SqlDatabaseOptions
    {
        public PostreSqlDatabaseOptions()
        {
            StartDelimiter = EndDelimiter = "\"";
            IsUsingSchemas = true;
        }
    }
}