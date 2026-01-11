using OsService.Domain.Enums;
using MediatR;

namespace OsService.Services.V1.UploadAttachment;

public sealed record UploadAttachmentCommand(
    Guid ServiceOrderId,
    AttachmentType Type,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream FileStream
) : IRequest<Guid>;
