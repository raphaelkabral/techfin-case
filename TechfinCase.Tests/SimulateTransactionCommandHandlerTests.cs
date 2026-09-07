using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Transactions.SimulateTransaction;
using TechfinCase.Domain.Entities;
using TechfinCase.Domain.Events;
using Xunit;

namespace TechfinCase.Tests;

public sealed class SimulateTransactionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeny_WhenClientDoesNotExist()
    {
        var clients = new Mock<IClientRepository>();
        clients
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = CreateHandler(clients);

        var result = await handler.Handle(
            new SimulateTransactionCommand(Guid.NewGuid().ToString(), 10),
            CancellationToken.None);

        Assert.Equal("NEGADO", result.Status);
    }

    [Fact]
    public async Task Handle_ShouldDeny_WhenAmountExceedsLimit()
    {
        var clientId = Guid.NewGuid();
        var clients = new Mock<IClientRepository>();
        clients
            .Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                CreditLimit = 50
            });

        var handler = CreateHandler(clients);

        var result = await handler.Handle(
            new SimulateTransactionCommand(clientId.ToString(), 100),
            CancellationToken.None);

        Assert.Equal("NEGADO", result.Status);
    }

    [Fact]
    public async Task Handle_ShouldApprove_PersistAndPublishMessage()
    {
        var clientId = Guid.NewGuid();
        var clients = new Mock<IClientRepository>();
        clients
            .Setup(x => x.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client
            {
                Id = clientId,
                CreditLimit = 100
            });

        var transactions = new Mock<ITransactionRepository>();
        var publisher = new Mock<IMessagePublisher>();

        var handler = new SimulateTransactionCommandHandler(
            clients.Object,
            transactions.Object,
            publisher.Object);

        var result = await handler.Handle(
            new SimulateTransactionCommand(clientId.ToString(), 50),
            CancellationToken.None);

        Assert.Equal("APROVADO", result.Status);
        Assert.NotNull(result.IdTransacao);

        transactions.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);

        publisher.Verify(
            x => x.PublishAsync(
                It.Is<AuthorizedTransactionEvent>(e =>
                    e.ClientId == clientId && e.Amount == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SimulateTransactionCommandHandler CreateHandler(
        Mock<IClientRepository> clients) =>
        new(
            clients.Object,
            new Mock<ITransactionRepository>().Object,
            new Mock<IMessagePublisher>().Object);
}
