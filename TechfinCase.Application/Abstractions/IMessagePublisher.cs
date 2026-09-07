using TechfinCase.Domain.Events;

namespace TechfinCase.Application.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync(AuthorizedTransactionEvent message, CancellationToken cancellationToken = default);
}
