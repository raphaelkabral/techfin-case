namespace TechfinCase.Domain.Events;

public sealed record AuthorizedTransactionEvent(Guid TransactionId, Guid ClientId, decimal Amount);
