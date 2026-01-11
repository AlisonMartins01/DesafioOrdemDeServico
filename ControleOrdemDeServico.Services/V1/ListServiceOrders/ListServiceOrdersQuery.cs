using MediatR;
using OsService.Domain.Entities;
using OsService.Domain.Enums;

namespace OsService.Services.V1.ListServiceOrders;

public sealed record ListServiceOrdersQuery(
    Guid? CustomerId,
    ServiceOrderStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate
) : IRequest<IEnumerable<ServiceOrderEntity>>;
