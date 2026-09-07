using TechfinCase.Domain.Entities;

namespace TechfinCase.Application.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
