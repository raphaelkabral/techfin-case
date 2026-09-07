namespace TechfinCase.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ClientId { get; init; }
    public decimal Amount { get; init; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
