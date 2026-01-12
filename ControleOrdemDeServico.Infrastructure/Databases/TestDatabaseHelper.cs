using Dapper;

namespace OsService.Infrastructure.Databases;

public class TestDatabaseHelper(IDefaultSqlConnectionFactory connectionFactory)
{
    public async Task ClearAllTablesAsync(CancellationToken ct = default)
    {
        const string clearSql = """
DELETE FROM dbo.Attachments;
DELETE FROM dbo.ServiceOrders;
DBCC CHECKIDENT ('dbo.ServiceOrders', RESEED, 999);
DELETE FROM dbo.Customers;
""";

        using var conn = connectionFactory.Create();
        await conn.ExecuteAsync(new CommandDefinition(clearSql, cancellationToken: ct));
    }
}
