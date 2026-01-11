using OsService.Domain.Enums;
using MediatR;

namespace OsService.Services.V1.UpdateServiceOrderStatus;

public sealed record UpdateServiceOrderStatusCommand(Guid ServiceOrderId, ServiceOrderStatus NewStatus) : IRequest;
