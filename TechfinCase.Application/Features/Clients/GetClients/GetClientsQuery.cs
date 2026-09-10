using MediatR;

namespace TechfinCase.Application.Features.Clients.GetClients;

public sealed record GetClientsQuery : IRequest<IReadOnlyList<ClientResponse>>;

public sealed record ClientResponse(string IdCliente, string Nome, string Cpf, decimal ValorLimite);
