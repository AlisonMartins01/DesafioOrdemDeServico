using OsService.Services.V1.OpenServiceOrder;
using OsService.Services.V1.UpdateServiceOrderStatus;
using OsService.Services.V1.UpdateServiceOrderPrice;
using OsService.Services.V1.UploadAttachment;
using OsService.Services.V1.ListServiceOrders;
using OsService.Infrastructure.Repository;
using OsService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace OsService.ApiService.Controllers;

[ApiController]
[Route("v1/service-orders")]
public sealed class ServiceOrdersController(
    IMediator mediator,
    IServiceOrderRepository serviceOrderRepository,
    IServiceOrderAttachmentRepository attachmentRepository
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? customerId,
        [FromQuery] ServiceOrderStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var query = new ListServiceOrdersQuery(customerId, status, fromDate, toDate);
        var serviceOrders = await mediator.Send(query, ct);
        return Ok(serviceOrders);
    }

    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenServiceOrderCommand cmd, CancellationToken ct)
    {
        var (id, number) = await mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id, number });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(id, ct);

        if (serviceOrder is null)
            return NotFound();

        return Ok(serviceOrder);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var command = new UpdateServiceOrderStatusCommand(id, request.Status);
        await mediator.Send(command, ct);
        return Ok();
    }

    [HttpPut("{id:guid}/price")]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request, CancellationToken ct)
    {
        var command = new UpdateServiceOrderPriceCommand(id, request.Price);
        await mediator.Send(command, ct);
        return Ok();
    }

    [HttpPost("{id:guid}/attachments/before")]
    public async Task<IActionResult> UploadBeforePhoto(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "File is required" });

        await using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            id,
            AttachmentType.Before,
            file.FileName,
            file.ContentType,
            file.Length,
            stream
        );

        var attachmentId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAttachments), new { id }, new { attachmentId });
    }

    [HttpPost("{id:guid}/attachments/after")]
    public async Task<IActionResult> UploadAfterPhoto(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "File is required" });

        await using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            id,
            AttachmentType.After,
            file.FileName,
            file.ContentType,
            file.Length,
            stream
        );

        var attachmentId = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAttachments), new { id }, new { attachmentId });
    }

    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> GetAttachments(Guid id, CancellationToken ct)
    {
        var attachments = await attachmentRepository.GetByServiceOrderIdAsync(id, ct);
        return Ok(attachments);
    }

    [HttpGet("attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid attachmentId, CancellationToken ct)
    {
        var attachment = await attachmentRepository.GetByIdAsync(attachmentId, ct);

        if (attachment is null)
            return NotFound();

        if (!System.IO.File.Exists(attachment.StoragePath))
            return NotFound(new { error = "File not found on storage" });

        var fileStream = new FileStream(attachment.StoragePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, attachment.ContentType, attachment.FileName);
    }
}

public record UpdateStatusRequest(ServiceOrderStatus Status);
public record UpdatePriceRequest(decimal Price);
