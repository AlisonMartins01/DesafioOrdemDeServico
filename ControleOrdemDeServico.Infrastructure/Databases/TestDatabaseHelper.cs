using Dapper;

namespace OsService.Infrastructure.Databases;

/// <summary>
/// Helper class for test database operations.
/// Used by integration tests to reset database state between tests.
/// </summary>
public class TestDatabaseHelper(IDefaultSqlConnectionFactory connectionFactory)
{
    /// <summary>
    /// Clears all data from tables and resets identity seed for ServiceOrders.
    /// Maintains referential integrity by deleting in correct order.
    /// </summary>
    public async Task ClearAllTablesAsync(CancellationToken ct = default)
    {
        const string clearSql = """
-- Delete in correct order due to foreign keys
DELETE FROM dbo.Attachments;
DELETE FROM dbo.ServiceOrders;
DBCC CHECKIDENT ('dbo.ServiceOrders', RESEED, 999); -- Reset identity to 1000
DELETE FROM dbo.Customers;
""";

        using var conn = connectionFactory.Create();
        await conn.ExecuteAsync(new CommandDefinition(clearSql, cancellationToken: ct));
    }
}
