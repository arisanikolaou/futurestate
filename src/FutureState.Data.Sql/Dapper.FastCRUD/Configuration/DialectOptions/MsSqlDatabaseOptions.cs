namespace Dapper.FastCrud.Configuration.DialectOptions
{
    internal class MsSqlDatabaseOptions : SqlDatabaseOptions
    {
        public MsSqlDatabaseOptions()
        {
            StartDelimiter = "[";
            EndDelimiter = "]";
            IsUsingSchemas = true;
        }
    }
}