using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OsService.Services.V1.OpenServiceOrder;

public sealed class OpenServiceOrderHandler(
    ICustomerRepository customers,
    IServiceOrderRepository serviceOrders,
    ILogger<OpenServiceOrderHandler> logger
) : IRequestHandler<OpenServiceOrderCommand, (Guid Id, int Number)>
{
    public async Task<(Guid Id, int Number)> Handle(OpenServiceOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Opening service order for customer: {CustomerId}", request.CustomerId);

        var exists = await customers.ExistsAsync(request.CustomerId, cancellationToken);
        if (!exists)
        {
            logger.LogWarning("Service order creation failed: Customer not found - {CustomerId}", request.CustomerId);
            throw new KeyNotFoundException("Customer not found.");
        }

        var serviceOrder = ServiceOrderEntity.Open(
            customerId: request.CustomerId,
            description: request.Description
        );

        var (id, number) = await serviceOrders.InsertAndReturnNumberAsync(serviceOrder, cancellationToken);

        logger.LogInformation("Service order opened successfully - Id: {ServiceOrderId}, Number: {ServiceOrderNumber}, CustomerId: {CustomerId}",
            id, number, request.CustomerId);

        return (id, number);
    }
}
