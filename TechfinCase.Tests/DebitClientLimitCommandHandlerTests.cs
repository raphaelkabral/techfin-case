using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Clients.DebitClientLimit;
using Xunit;

namespace TechfinCase.Tests;

public sealed class DebitClientLimitCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDebitLimit_AndInvalidateCache()
    {
        var clientId = Guid.NewGuid();
        var repository = new Mock<IClientRepository>();
        repository
            .Setup(x => x.DebitLimitAsync(clientId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var cache = new Mock<ICacheService>();
        var handler = new DebitClientLimitCommandHandler(repository.Object, cache.Object);

        var result = await handler.Handle(
            new DebitClientLimitCommand(clientId, 50),
            CancellationToken.None);

        Assert.True(result);
        cache.Verify(x => x.Remove("clients:all"), Times.Once);
    }
}
