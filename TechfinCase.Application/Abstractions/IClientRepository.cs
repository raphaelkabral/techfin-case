using TechfinCase.Domain.Entities;

namespace TechfinCase.Application.Abstractions;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> DebitLimitAsync(Guid clientId, decimal amount, CancellationToken cancellationToken = default);
}
