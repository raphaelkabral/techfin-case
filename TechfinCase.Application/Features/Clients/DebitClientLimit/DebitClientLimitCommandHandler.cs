using MediatR;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Common;

namespace TechfinCase.Application.Features.Clients.DebitClientLimit;

public sealed class DebitClientLimitCommandHandler(
    IClientRepository clientRepository,
    ICacheService cacheService)
    : IRequestHandler<DebitClientLimitCommand, bool>
{
    public async Task<bool> Handle(DebitClientLimitCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return false;
        }

        var updated = await clientRepository.DebitLimitAsync(request.ClientId, request.Amount, cancellationToken);

        if (updated)
        {
            cacheService.Remove(CacheKeys.AllClients);
        }

        return updated;
    }
}
