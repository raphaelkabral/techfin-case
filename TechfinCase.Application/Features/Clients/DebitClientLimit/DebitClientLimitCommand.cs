using MediatR;

namespace TechfinCase.Application.Features.Clients.DebitClientLimit;

public sealed record DebitClientLimitCommand(Guid ClientId, decimal Amount) : IRequest<bool>;
