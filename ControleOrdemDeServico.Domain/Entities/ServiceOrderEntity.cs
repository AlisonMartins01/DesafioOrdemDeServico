using OsService.Domain.Enums;

namespace OsService.Domain.Entities;

public sealed class ServiceOrderEntity
{
    // Private constructor - force use of factory method
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

    /// <summary>
    /// Factory method to create a new Service Order
    /// </summary>
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

    /// <summary>
    /// Start service order execution
    /// </summary>
    public void Start()
    {
        if (Status != ServiceOrderStatus.Open)
            throw new InvalidOperationException($"Cannot start service order. Current status: {Status}. Expected: Open.");

        Status = ServiceOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Finish service order
    /// </summary>
    public void Finish()
    {
        if (Status != ServiceOrderStatus.InProgress)
            throw new InvalidOperationException($"Cannot finish service order. Current status: {Status}. Expected: InProgress.");

        if (Price is null)
            throw new InvalidOperationException("Cannot finish service order without a price.");

        Status = ServiceOrderStatus.Finished;
        FinishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update service order price
    /// </summary>
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

    /// <summary>
    /// Change status with validation
    /// </summary>
    public void ChangeStatus(ServiceOrderStatus newStatus)
    {
        // Validate transition
        var isValidTransition = (Status, newStatus) switch
        {
            (ServiceOrderStatus.Open, ServiceOrderStatus.InProgress) => true,
            (ServiceOrderStatus.InProgress, ServiceOrderStatus.Finished) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException($"Invalid status transition from {Status} to {newStatus}.");

        // Apply transition
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

    /// <summary>
    /// Reconstitute entity from database (for repository use only)
    /// </summary>
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

    /// <summary>
    /// Set number after insertion (called by repository)
    /// </summary>
    internal void SetNumber(int number)
    {
        if (Number != 0)
            throw new InvalidOperationException("Number already set.");

        Number = number;
    }
}
