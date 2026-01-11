using OsService.Services.V1.CreateCustomer;
using OsService.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OsService.ApiService.Controllers;

[ApiController]
[Route("v1/customers")]
public sealed class CustomersController(IMediator mediator, ICustomerRepository customerRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? phone, [FromQuery] string? document, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(document))
            return BadRequest(new { error = "Either phone or document must be provided" });

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var customer = await customerRepository.GetByPhoneAsync(phone, ct);
            if (customer is not null)
                return Ok(customer);
        }

        if (!string.IsNullOrWhiteSpace(document))
        {
            var customer = await customerRepository.GetByDocumentAsync(document, ct);
            if (customer is not null)
                return Ok(customer);
        }

        return NotFound();
    }
}