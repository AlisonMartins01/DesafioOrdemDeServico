using MediatR;

namespace OsService.Services.V1.UpdateServiceOrderPrice;

public sealed record UpdateServiceOrderPriceCommand(Guid ServiceOrderId, decimal Price) : IRequest;
