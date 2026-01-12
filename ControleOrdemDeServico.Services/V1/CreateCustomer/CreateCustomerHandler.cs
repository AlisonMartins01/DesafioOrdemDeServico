using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OsService.Services.V1.CreateCustomer;

public sealed class CreateCustomerHandler(
    ICustomerRepository repo,
    ILogger<CreateCustomerHandler> logger)
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating customer with name: {CustomerName}", request.Name);

        if (!string.IsNullOrWhiteSpace(request.Document))
        {
            var existingByDocument = await repo.GetByDocumentAsync(request.Document.Trim(), cancellationToken);
            if (existingByDocument is not null)
            {
                logger.LogWarning("Customer creation failed: Duplicate document - {Document}", request.Document);
                throw new InvalidOperationException("A customer with this document already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var existingByPhone = await repo.GetByPhoneAsync(request.Phone.Trim(), cancellationToken);
            if (existingByPhone is not null)
            {
                logger.LogWarning("Customer creation failed: Duplicate phone - {Phone}", request.Phone);
                throw new InvalidOperationException("A customer with this phone already exists.");
            }
        }

        var customer = CustomerEntity.Create(
            name: request.Name,
            phone: request.Phone,
            email: request.Email,
            document: request.Document
        );

        await repo.InsertAsync(customer, cancellationToken);

        logger.LogInformation("Customer created successfully - Id: {CustomerId}, Name: {CustomerName}",
            customer.Id, customer.Name);

        return customer.Id;
    }
}
