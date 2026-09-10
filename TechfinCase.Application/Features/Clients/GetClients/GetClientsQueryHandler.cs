using MediatR;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Common;

namespace TechfinCase.Application.Features.Clients.GetClients;

public sealed class GetClientsQueryHandler(
    IClientRepository clientRepository,
    ICacheService cacheService)
    : IRequestHandler<GetClientsQuery, IReadOnlyList<ClientResponse>>
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<IReadOnlyList<ClientResponse>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        if (cacheService.TryGet<IReadOnlyList<ClientResponse>>(CacheKeys.AllClients, out var cachedClients) && cachedClients is not null)
        {
            return cachedClients;
        }

        var clients = await clientRepository.GetAllAsync(cancellationToken);

        var response = clients
            .Select(client => new ClientResponse(
                client.Id.ToString(),
                client.Name,
                client.Cpf,
                client.CreditLimit))
            .ToList();

        cacheService.Set(CacheKeys.AllClients, response, CacheExpiration);

        return response;
    }
}
