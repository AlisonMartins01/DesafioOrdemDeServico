using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using MediatR;

namespace OsService.Services.V1.UploadAttachment;

public sealed class UploadAttachmentHandler(
    IServiceOrderRepository serviceOrders,
    IServiceOrderAttachmentRepository attachments
) : IRequestHandler<UploadAttachmentCommand, Guid>
{
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png" };
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public async Task<Guid> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        var serviceOrder = await serviceOrders.GetByIdAsync(request.ServiceOrderId, ct);
        if (serviceOrder is null)
            throw new KeyNotFoundException("Service order not found.");

        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Only JPEG and PNG images are allowed.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only .jpg, .jpeg, and .png file extensions are allowed.");

        if (request.SizeBytes > MaxFileSizeBytes)
            throw new ArgumentException($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024}MB.");

        if (request.SizeBytes <= 0)
            throw new ArgumentException("File is empty.");

        var sanitizedFileName = SanitizeFileName(request.FileName);

        var attachmentId = Guid.NewGuid();
        var storagePath = GenerateStoragePath(request.ServiceOrderId, attachmentId, extension);

        var uploadDirectory = Path.GetDirectoryName(storagePath);
        if (!string.IsNullOrEmpty(uploadDirectory))
        {
            Directory.CreateDirectory(uploadDirectory);
        }

        await using (var fileStream = new FileStream(storagePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await request.FileStream.CopyToAsync(fileStream, ct);
        }

        var attachment = new ServiceOrderAttachment
        {
            Id = attachmentId,
            ServiceOrderId = request.ServiceOrderId,
            Type = request.Type,
            FileName = sanitizedFileName,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            StoragePath = storagePath,
            UploadedAt = DateTime.UtcNow
        };

        await attachments.InsertAsync(attachment, ct);

        return attachmentId;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 255 ? sanitized[..255] : sanitized;
    }

    private static string GenerateStoragePath(Guid serviceOrderId, Guid attachmentId, string extension)
    {
        var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data", "uploads");
        var fileName = $"{serviceOrderId}_{attachmentId}{extension}";
        return Path.Combine(baseDirectory, fileName);
    }
}
