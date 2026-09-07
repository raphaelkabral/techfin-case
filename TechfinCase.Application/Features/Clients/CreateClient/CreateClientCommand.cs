using MediatR;

namespace TechfinCase.Application.Features.Clients.CreateClient;

public sealed record CreateClientCommand(
    string Nome,
    string Cpf,
    decimal ValorLimite) : IRequest<CreateClientResponse>;

public sealed record CreateClientResponse(
    string? IdCliente,
    string Status,
    string? DetalheErro = null);
