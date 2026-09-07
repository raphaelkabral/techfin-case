using MediatR;

namespace TechfinCase.Application.Features.Transactions.SimulateTransaction;

public sealed record SimulateTransactionCommand(string IdCliente, decimal ValorSimulacao) : IRequest<SimulateTransactionResponse>;

public sealed record SimulateTransactionResponse(string Status, string? IdTransacao = null);
