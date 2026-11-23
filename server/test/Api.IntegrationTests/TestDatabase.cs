using LinqToDB;
using Infra;
using Npgsql;

namespace Api.IntegrationTests;

internal static class TestDatabase
{
    private static readonly string BaseConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=business_esbjerg;Username=postgres;Password=postgres";

    public static string CreateSchema()
    {
        var schema = $"test_{Guid.NewGuid():N}";
        Run($"CREATE SCHEMA {schema}");
        return schema;
    }

    public static void DropSchema(string schema)
    {
        Run($"DROP SCHEMA {schema} CASCADE");
    }

    public static string ConnectionString(string schema)
    {
        return new NpgsqlConnectionStringBuilder(BaseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;
    }

    public static MyDatabaseConnection CreateConnection(string schema)
    {
        var options = new DataOptions().UsePostgreSQL(ConnectionString(schema));
        return new MyDatabaseConnection(new DataOptions<MyDatabaseConnection>(options));
    }

    private static void Run(string sql)
    {
        using var connection = new NpgsqlConnection(BaseConnectionString);
        connection.Open();
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }
}