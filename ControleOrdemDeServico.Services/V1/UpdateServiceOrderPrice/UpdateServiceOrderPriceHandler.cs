using OsService.Infrastructure.Repository;
using MediatR;

namespace OsService.Services.V1.UpdateServiceOrderPrice;

public sealed class UpdateServiceOrderPriceHandler(IServiceOrderRepository serviceOrders)
    : IRequestHandler<UpdateServiceOrderPriceCommand>
{
    public async Task Handle(UpdateServiceOrderPriceCommand request, CancellationToken ct)
    {
        var serviceOrder = await serviceOrders.GetByIdAsync(request.ServiceOrderId, ct);

        if (serviceOrder is null)
            throw new KeyNotFoundException("Service order not found.");

        serviceOrder.UpdatePrice(request.Price);

        await serviceOrders.UpdatePriceAsync(
            request.ServiceOrderId,
            serviceOrder.Price!.Value,
            serviceOrder.UpdatedPriceAt!.Value,
            ct);
    }
}
