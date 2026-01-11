using OsService.Domain.Entities;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderAttachmentRepository
{
    Task InsertAsync(ServiceOrderAttachment attachment, CancellationToken ct);
    Task<List<ServiceOrderAttachment>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken ct);
    Task<ServiceOrderAttachment?> GetByIdAsync(Guid id, CancellationToken ct);
}
