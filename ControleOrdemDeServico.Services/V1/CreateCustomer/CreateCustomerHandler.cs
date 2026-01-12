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
    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        logger.LogInformation("Creating customer with name: {CustomerName}", request.Name);

        try
        {
            if (!string.IsNullOrWhiteSpace(request.Document))
            {
                var existingByDocument = await repo.GetByDocumentAsync(request.Document.Trim(), ct);
                if (existingByDocument is not null)
                {
                    logger.LogWarning("Customer creation failed: Duplicate document - {Document}", request.Document);
                    throw new InvalidOperationException("A customer with this document already exists.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var existingByPhone = await repo.GetByPhoneAsync(request.Phone.Trim(), ct);
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

            await repo.InsertAsync(customer, ct);

            logger.LogInformation("Customer created successfully - Id: {CustomerId}, Name: {CustomerName}",
                customer.Id, customer.Name);

            return customer.Id;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Customer creation failed: {Message}", ex.Message);
            throw;
        }
    }
}
