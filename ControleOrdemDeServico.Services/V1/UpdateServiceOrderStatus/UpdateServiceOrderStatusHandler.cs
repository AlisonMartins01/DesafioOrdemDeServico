using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OsService.Services.V1.UpdateServiceOrderStatus;

public sealed class UpdateServiceOrderStatusHandler(
    IServiceOrderRepository serviceOrders,
    ILogger<UpdateServiceOrderStatusHandler> logger)
    : IRequestHandler<UpdateServiceOrderStatusCommand>
{
    public async Task Handle(UpdateServiceOrderStatusCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating status for service order: {ServiceOrderId} to {NewStatus}",
            request.ServiceOrderId, request.NewStatus);

        var serviceOrder = await serviceOrders.GetByIdAsync(request.ServiceOrderId, cancellationToken);

        if (serviceOrder is null)
        {
            logger.LogWarning("Status update failed: Service order not found - {ServiceOrderId}", request.ServiceOrderId);
            throw new KeyNotFoundException("Service order not found.");
        }

        var currentStatus = serviceOrder.Status;

        serviceOrder.ChangeStatus(request.NewStatus);

        await serviceOrders.UpdateStatusAsync(
            request.ServiceOrderId,
            serviceOrder.Status,
            serviceOrder.StartedAt,
            serviceOrder.FinishedAt,
            cancellationToken);

        logger.LogInformation("Service order status updated successfully - Id: {ServiceOrderId}, Status: {CurrentStatus} -> {NewStatus}",
            request.ServiceOrderId, currentStatus, request.NewStatus);
    }
}
