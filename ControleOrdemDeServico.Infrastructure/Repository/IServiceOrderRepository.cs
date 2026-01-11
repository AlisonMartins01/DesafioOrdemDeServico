using OsService.Domain.Entities;
using OsService.Domain.Enums;

namespace OsService.Infrastructure.Repository;

public interface IServiceOrderRepository
{
    Task<(Guid id, int number)> InsertAndReturnNumberAsync(ServiceOrderEntity so, CancellationToken ct);
    Task<ServiceOrderEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task UpdateStatusAsync(Guid id, ServiceOrderStatus status, DateTime? startedAt, DateTime? finishedAt, CancellationToken ct);
    Task UpdatePriceAsync(Guid id, decimal price, DateTime updatedPriceAt, CancellationToken ct);
    Task<IEnumerable<ServiceOrderEntity>> ListAsync(Guid? customerId, ServiceOrderStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
}
