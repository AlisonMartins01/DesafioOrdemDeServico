using OsService.Domain.Entities;
using OsService.Infrastructure.Databases;
using Dapper;

namespace OsService.Infrastructure.Repository;

public sealed class CustomerRepository(IDefaultSqlConnectionFactory factory) : ICustomerRepository
{
    public async Task InsertAsync(CustomerEntity customer, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.Customers (Id, Name, Phone, Email, Document, CreatedAt)
VALUES (@Id, @Name, @Phone, @Email, @Document, @CreatedAt);";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, customer, cancellationToken: ct));
    }

    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Name, Phone, Email, Document, CreatedAt
FROM dbo.Customers
WHERE Id = @Id;";

        using var conn = factory.Create();
        var result = await conn.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        if (result is null)
            return null;

        return CustomerEntity.Reconstitute(
            id: result.Id,
            name: result.Name,
            phone: result.Phone,
            email: result.Email,
            document: result.Document,
            createdAt: result.CreatedAt
        );
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        const string sql = "SELECT 1 FROM dbo.Customers WHERE Id = @Id;";
        using var conn = factory.Create();
        var exists = await conn.QueryFirstOrDefaultAsync<int?>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return exists.HasValue;
    }

    public async Task<CustomerEntity?> GetByPhoneAsync(string phone, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Name, Phone, Email, Document, CreatedAt
FROM dbo.Customers
WHERE Phone = @Phone;";

        using var conn = factory.Create();
        var result = await conn.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Phone = phone }, cancellationToken: ct));

        if (result is null)
            return null;

        return CustomerEntity.Reconstitute(
            id: result.Id,
            name: result.Name,
            phone: result.Phone,
            email: result.Email,
            document: result.Document,
            createdAt: result.CreatedAt
        );
    }

    public async Task<CustomerEntity?> GetByDocumentAsync(string document, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Name, Phone, Email, Document, CreatedAt
FROM dbo.Customers
WHERE Document = @Document;";

        using var conn = factory.Create();
        var result = await conn.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Document = document }, cancellationToken: ct));

        if (result is null)
            return null;

        return CustomerEntity.Reconstitute(
            id: result.Id,
            name: result.Name,
            phone: result.Phone,
            email: result.Email,
            document: result.Document,
            createdAt: result.CreatedAt
        );
    }
}
