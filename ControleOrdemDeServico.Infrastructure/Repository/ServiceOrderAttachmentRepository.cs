using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Databases;
using Dapper;

namespace OsService.Infrastructure.Repository;

public sealed class ServiceOrderAttachmentRepository(IDefaultSqlConnectionFactory factory)
    : IServiceOrderAttachmentRepository
{
    public async Task InsertAsync(ServiceOrderAttachment attachment, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.ServiceOrderAttachments (Id, ServiceOrderId, Type, FileName, ContentType, SizeBytes, StoragePath, UploadedAt)
VALUES (@Id, @ServiceOrderId, @Type, @FileName, @ContentType, @SizeBytes, @StoragePath, @UploadedAt);";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            attachment.Id,
            attachment.ServiceOrderId,
            Type = (int)attachment.Type,
            attachment.FileName,
            attachment.ContentType,
            attachment.SizeBytes,
            attachment.StoragePath,
            attachment.UploadedAt
        }, cancellationToken: ct));
    }

    public async Task<List<ServiceOrderAttachment>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, ServiceOrderId, Type = CAST(Type AS INT), FileName, ContentType, SizeBytes, StoragePath, UploadedAt
FROM dbo.ServiceOrderAttachments
WHERE ServiceOrderId = @ServiceOrderId
ORDER BY UploadedAt;";

        using var conn = factory.Create();
        var results = await conn.QueryAsync<dynamic>(
            new CommandDefinition(sql, new { ServiceOrderId = serviceOrderId }, cancellationToken: ct));

        return results.Select(r => new ServiceOrderAttachment
        {
            Id = r.Id,
            ServiceOrderId = r.ServiceOrderId,
            Type = (AttachmentType)(int)r.Type,
            FileName = r.FileName,
            ContentType = r.ContentType,
            SizeBytes = r.SizeBytes,
            StoragePath = r.StoragePath,
            UploadedAt = r.UploadedAt
        }).ToList();
    }

    public async Task<ServiceOrderAttachment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, ServiceOrderId, Type = CAST(Type AS INT), FileName, ContentType, SizeBytes, StoragePath, UploadedAt
FROM dbo.ServiceOrderAttachments
WHERE Id = @Id;";

        using var conn = factory.Create();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));

        if (result is null)
            return null;

        return new ServiceOrderAttachment
        {
            Id = result.Id,
            ServiceOrderId = result.ServiceOrderId,
            Type = (AttachmentType)(int)result.Type,
            FileName = result.FileName,
            ContentType = result.ContentType,
            SizeBytes = result.SizeBytes,
            StoragePath = result.StoragePath,
            UploadedAt = result.UploadedAt
        };
    }
}
