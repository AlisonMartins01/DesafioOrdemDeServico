using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Databases;
using Dapper;

namespace OsService.Infrastructure.Repository;

public sealed class ServiceOrderRepository(IDefaultSqlConnectionFactory factory) : IServiceOrderRepository
{
    public async Task<(Guid id, int number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.ServiceOrders (Id, CustomerId, Description, Status, OpenedAt, StartedAt, FinishedAt, Price, Coin)
OUTPUT INSERTED.Id, INSERTED.Number
VALUES (@Id, @CustomerId, @Description, @Status, @OpenedAt, @StartedAt, @FinishedAt, @Price, @Coin);";

        using var conn = factory.Create();
        var row = await conn.QuerySingleAsync<(Guid Id, int Number)>(
            new CommandDefinition(sql, new
            {
                so.Id,
                so.CustomerId,
                so.Description,
                Status = (int)so.Status,
                so.OpenedAt,
                so.StartedAt,
                so.FinishedAt,
                so.Price,
                so.Coin
            }, cancellationToken: ct));

        return (row.Id, row.Number);
    }

    public async Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, Number, CustomerId, Description,
       Status = CAST(Status AS INT),
       OpenedAt, StartedAt, FinishedAt, Price, Coin, UpdatedPriceAt
FROM dbo.ServiceOrders
WHERE Id = @Id;";

        using var conn = factory.Create();
        var raw = await conn.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        if (raw is null) return null;

        return ServiceOrderEntity.Reconstitute(
            id: raw.Id,
            number: raw.Number,
            customerId: raw.CustomerId,
            description: raw.Description,
            status: (ServiceOrderStatus)(int)raw.Status,
            openedAt: raw.OpenedAt,
            startedAt: raw.StartedAt,
            finishedAt: raw.FinishedAt,
            price: raw.Price,
            coin: raw.Coin,
            updatedPriceAt: raw.UpdatedPriceAt
        );
    }

    public async Task UpdateStatusAsync(Guid id, ServiceOrderStatus status, DateTime? startedAt, DateTime? finishedAt, CancellationToken ct)
    {
        const string sql = @"
UPDATE dbo.ServiceOrders
SET Status = @Status, StartedAt = @StartedAt, FinishedAt = @FinishedAt
WHERE Id = @Id;";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = id,
            Status = (int)status,
            StartedAt = startedAt,
            FinishedAt = finishedAt
        }, cancellationToken: ct));
    }

    public async Task UpdatePriceAsync(Guid id, decimal price, DateTime updatedPriceAt, CancellationToken ct)
    {
        const string sql = @"
UPDATE dbo.ServiceOrders
SET Price = @Price, UpdatedPriceAt = @UpdatedPriceAt
WHERE Id = @Id;";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = id,
            Price = price,
            UpdatedPriceAt = updatedPriceAt
        }, cancellationToken: ct));
    }

    public async Task<IEnumerable<ServiceOrderEntity>> ListAsync(
        Guid? customerId,
        ServiceOrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        var sql = @"
SELECT Id, Number, CustomerId, Description,
       Status = CAST(Status AS INT),
       OpenedAt, StartedAt, FinishedAt, Price, Coin, UpdatedPriceAt
FROM dbo.ServiceOrders
WHERE 1=1";

        var parameters = new DynamicParameters();

        if (customerId.HasValue)
        {
            sql += " AND CustomerId = @CustomerId";
            parameters.Add("CustomerId", customerId.Value);
        }

        if (status.HasValue)
        {
            sql += " AND Status = @Status";
            parameters.Add("Status", (int)status.Value);
        }

        if (fromDate.HasValue)
        {
            sql += " AND OpenedAt >= @FromDate";
            parameters.Add("FromDate", fromDate.Value);
        }

        if (toDate.HasValue)
        {
            sql += " AND OpenedAt <= @ToDate";
            parameters.Add("ToDate", toDate.Value);
        }

        sql += " ORDER BY Number DESC;";

        using var conn = factory.Create();
        var results = await conn.QueryAsync<dynamic>(
            new CommandDefinition(sql, parameters, cancellationToken: ct));

        var serviceOrders = new List<ServiceOrderEntity>();
        foreach (var raw in results)
        {
            serviceOrders.Add(ServiceOrderEntity.Reconstitute(
                id: raw.Id,
                number: raw.Number,
                customerId: raw.CustomerId,
                description: raw.Description,
                status: (ServiceOrderStatus)(int)raw.Status,
                openedAt: raw.OpenedAt,
                startedAt: raw.StartedAt,
                finishedAt: raw.FinishedAt,
                price: raw.Price,
                coin: raw.Coin,
                updatedPriceAt: raw.UpdatedPriceAt
            ));
        }

        return serviceOrders;
    }
}
