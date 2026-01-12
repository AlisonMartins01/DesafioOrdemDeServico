using System.Text.RegularExpressions;

namespace OsService.Domain.Entities;

public sealed partial class CustomerEntity
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    private CustomerEntity() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Document { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static CustomerEntity Create(
        string name,
        string? phone = null,
        string? email = null,
        string? document = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required and cannot be only whitespace.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 150)
            throw new ArgumentException("Name must be between 2 and 150 characters.", nameof(name));

        var trimmedPhone = phone?.Trim();
        if (trimmedPhone is not null && trimmedPhone.Length > 30)
            throw new ArgumentException("Phone must be at most 30 characters.", nameof(phone));

        var trimmedEmail = email?.Trim();
        if (!string.IsNullOrEmpty(trimmedEmail))
        {
            if (trimmedEmail.Length > 120)
                throw new ArgumentException("Email must be at most 120 characters.", nameof(email));

            if (!EmailRegex().IsMatch(trimmedEmail))
                throw new ArgumentException("Email format is invalid.", nameof(email));
        }

        var trimmedDocument = document?.Trim();
        if (trimmedDocument is not null && trimmedDocument.Length > 30)
            throw new ArgumentException("Document must be at most 30 characters.", nameof(document));

        return new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = trimmedName,
            Phone = string.IsNullOrWhiteSpace(trimmedPhone) ? null : trimmedPhone,
            Email = string.IsNullOrWhiteSpace(trimmedEmail) ? null : trimmedEmail,
            Document = string.IsNullOrWhiteSpace(trimmedDocument) ? null : trimmedDocument,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CustomerEntity Reconstitute(
        Guid id,
        string name,
        string? phone,
        string? email,
        string? document,
        DateTime createdAt)
    {
        return new CustomerEntity
        {
            Id = id,
            Name = name,
            Phone = phone,
            Email = email,
            Document = document,
            CreatedAt = createdAt
        };
    }
}
