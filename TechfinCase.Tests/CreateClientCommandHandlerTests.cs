using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Clients.CreateClient;
using TechfinCase.Domain.Entities;
using Xunit;

namespace TechfinCase.Tests;

public sealed class CreateClientCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRejectNegativeLimit()
    {
        var repository = new Mock<IClientRepository>();
        var handler = new CreateClientCommandHandler(
            repository.Object,
            new Mock<ICacheService>().Object);

        var result = await handler.Handle(
            new CreateClientCommand("Ana", "12345678900", -1),
            CancellationToken.None);

        Assert.Equal("ERRO", result.Status);
        repository.Verify(
            x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRejectDuplicatedCpf()
    {
        var repository = new Mock<IClientRepository>();
        repository
            .Setup(x => x.GetByCpfAsync("12345678900", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client { Cpf = "12345678900" });

        var handler = new CreateClientCommandHandler(
            repository.Object,
            new Mock<ICacheService>().Object);

        var result = await handler.Handle(
            new CreateClientCommand("Ana", "123.456.789-00", 100),
            CancellationToken.None);

        Assert.Equal("ERRO", result.Status);
    }

    [Fact]
    public async Task Handle_ShouldCreateClient_AndInvalidateCache()
    {
        var repository = new Mock<IClientRepository>();
        repository
            .Setup(x => x.GetByCpfAsync("12345678900", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var cache = new Mock<ICacheService>();
        var handler = new CreateClientCommandHandler(repository.Object, cache.Object);

        var result = await handler.Handle(
            new CreateClientCommand("Ana", "123.456.789-00", 100),
            CancellationToken.None);

        Assert.Equal("OK", result.Status);
        Assert.NotNull(result.IdCliente);
        repository.Verify(
            x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
            Times.Once);
        cache.Verify(x => x.Remove("clients:all"), Times.Once);
    }
}
