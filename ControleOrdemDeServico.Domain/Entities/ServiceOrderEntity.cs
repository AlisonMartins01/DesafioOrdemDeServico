using OsService.Domain.Enums;

namespace OsService.Domain.Entities;

public sealed class ServiceOrderEntity
{
    private ServiceOrderEntity() { }

    public Guid Id { get; private set; }
    public int Number { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Description { get; private set; } = default!;
    public ServiceOrderStatus Status { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public decimal? Price { get; private set; }
    public string? Coin { get; private set; }
    public DateTime? UpdatedPriceAt { get; private set; }

    public static ServiceOrderEntity Open(Guid customerId, string description)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < 1 || trimmedDescription.Length > 500)
            throw new ArgumentException("Description must be between 1 and 500 characters.", nameof(description));

        return new ServiceOrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Description = trimmedDescription,
            Status = ServiceOrderStatus.Open,
            OpenedAt = DateTime.UtcNow,
            Coin = "BRL"
        };
    }

    public void Start()
    {
        if (Status != ServiceOrderStatus.Open)
            throw new InvalidOperationException($"Cannot start service order. Current status: {Status}. Expected: Open.");

        Status = ServiceOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Finish()
    {
        if (Status != ServiceOrderStatus.InProgress)
            throw new InvalidOperationException($"Cannot finish service order. Current status: {Status}. Expected: InProgress.");

        if (Price is null)
            throw new InvalidOperationException("Cannot finish service order without a price.");

        Status = ServiceOrderStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal price, string? coin = "BRL")
    {
        if (Status == ServiceOrderStatus.Finished)
            throw new InvalidOperationException("Cannot update price of a finished service order.");

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        Price = price;
        Coin = coin ?? "BRL";
        UpdatedPriceAt = DateTime.UtcNow;
    }

    public void ChangeStatus(ServiceOrderStatus newStatus)
    {
        var isValidTransition = (Status, newStatus) switch
        {
            (ServiceOrderStatus.Open, ServiceOrderStatus.InProgress) => true,
            (ServiceOrderStatus.InProgress, ServiceOrderStatus.Finished) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}.");

        switch (newStatus)
        {
            case ServiceOrderStatus.InProgress:
                Start();
                break;
            case ServiceOrderStatus.Finished:
                Finish();
                break;
            default:
                throw new InvalidOperationException($"Unsupported status: {newStatus}");
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "S107:Methods should not have too many parameters",
        Justification = "Método de reconstituição DDD que mapeia todas as propriedades da entidade vindas do banco de dados")]
    public static ServiceOrderEntity Reconstitute(
        Guid id,
        int number,
        Guid customerId,
        string description,
        ServiceOrderStatus status,
        DateTime openedAt,
        DateTime? startedAt,
        DateTime? finishedAt,
        decimal? price,
        string? coin,
        DateTime? updatedPriceAt)
    {
        return new ServiceOrderEntity
        {
            Id = id,
            Number = number,
            CustomerId = customerId,
            Description = description,
            Status = status,
            OpenedAt = openedAt,
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            Price = price,
            Coin = coin,
            UpdatedPriceAt = updatedPriceAt
        };
    }

    internal void SetNumber(int number)
    {
        if (Number != 0)
            throw new InvalidOperationException("Number already set.");

        Number = number;
    }
}
