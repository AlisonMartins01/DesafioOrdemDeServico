using MediatR;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;

namespace OsService.Services.V1.ListServiceOrders;

public sealed class ListServiceOrdersHandler(IServiceOrderRepository repo)
    : IRequestHandler<ListServiceOrdersQuery, IEnumerable<ServiceOrderEntity>>
{
    public async Task<IEnumerable<ServiceOrderEntity>> Handle(ListServiceOrdersQuery request, CancellationToken ct)
    {
        return await repo.ListAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            ct);
    }
}
