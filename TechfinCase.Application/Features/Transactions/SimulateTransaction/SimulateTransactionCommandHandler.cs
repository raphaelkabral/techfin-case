using MediatR;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Entities;
using TechfinCase.Domain.Events;

namespace TechfinCase.Application.Features.Transactions.SimulateTransaction;

public sealed class SimulateTransactionCommandHandler(
    IClientRepository clientRepository,
    ITransactionRepository transactionRepository,
    IMessagePublisher messagePublisher)
    : IRequestHandler<SimulateTransactionCommand, SimulateTransactionResponse>
{
    public async Task<SimulateTransactionResponse> Handle(
        SimulateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.IdCliente, out var clientId) || request.ValorSimulacao <= 0)
        {
            return Denied();
        }

        var client = await clientRepository.GetByIdAsync(clientId, cancellationToken);

        if (client is null || request.ValorSimulacao > client.CreditLimit)
        {
            return Denied();
        }

        var transaction = new Transaction
        {
            ClientId = client.Id,
            Amount = request.ValorSimulacao
        };

        await transactionRepository.AddAsync(transaction, cancellationToken);

        await messagePublisher.PublishAsync(
            new AuthorizedTransactionEvent(
                transaction.Id,
                transaction.ClientId,
                transaction.Amount),
            cancellationToken);

        return new SimulateTransactionResponse(
            "APROVADO",
            transaction.Id.ToString());
    }

    private static SimulateTransactionResponse Denied() => new("NEGADO");
}
