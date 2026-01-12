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
INSERT INTO dbo.ServiceOrderAttachments (Id, ServiceOrderId, AttachmentType, FileName, ContentType, FileSizeBytes, StoragePath, UploadedAt)
VALUES (@Id, @ServiceOrderId, @Type, @FileName, @ContentType, @FileSizeBytes, @StoragePath, @UploadedAt);";

        using var conn = factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(sql, new
        {
            attachment.Id,
            attachment.ServiceOrderId,
            Type = (int)attachment.Type,
            attachment.FileName,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.StoragePath,
            attachment.UploadedAt
        }, cancellationToken: ct));
    }

    public async Task<List<ServiceOrderAttachment>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, ServiceOrderId, AttachmentType = CAST(AttachmentType AS INT), FileName, ContentType, FileSizeBytes, StoragePath, UploadedAt
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
            Type = (AttachmentType)(int)r.AttachmentType,
            FileName = r.FileName,
            ContentType = r.ContentType,
            FileSizeBytes = r.FileSizeBytes,
            StoragePath = r.StoragePath,
            UploadedAt = r.UploadedAt
        }).ToList();
    }

    public async Task<ServiceOrderAttachment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, ServiceOrderId, AttachmentType = CAST(AttachmentType AS INT), FileName, ContentType, FileSizeBytes, StoragePath, UploadedAt
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
            Type = (AttachmentType)(int)result.AttachmentType,
            FileName = result.FileName,
            ContentType = result.ContentType,
            FileSizeBytes = result.FileSizeBytes,
            StoragePath = result.StoragePath,
            UploadedAt = result.UploadedAt
        };
    }
}
