using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Clients.GetClients;
using TechfinCase.Domain.Entities;
using Xunit;

namespace TechfinCase.Tests;

public sealed class GetClientsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnCachedClients_WhenCacheExists()
    {
        var cached = new List<ClientResponse>
        {
            new(Guid.NewGuid().ToString(), "Ana", "12345678900", 100)
        };

        var cache = new FakeCacheService();
        cache.Set("clients:all", cached, TimeSpan.FromMinutes(10));

        var repository = new Mock<IClientRepository>();
        var handler = new GetClientsQueryHandler(repository.Object, cache);

        var result = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        Assert.Same(cached, result);
        repository.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldLoadAndCacheClients_WhenCacheDoesNotExist()
    {
        var cache = new FakeCacheService();
        var repository = new Mock<IClientRepository>();
        repository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>
            {
                new()
                {
                    Name = "Ana",
                    Cpf = "12345678900",
                    CreditLimit = 100
                }
            });

        var handler = new GetClientsQueryHandler(repository.Object, cache);

        var result = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.True(cache.TryGet<IReadOnlyList<ClientResponse>>("clients:all", out var cached));
        Assert.NotNull(cached);
        Assert.Single(cached);
    }

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _cache = new();

        public bool TryGet<T>(string key, out T? value)
        {
            if (_cache.TryGetValue(key, out var item) && item is T typedItem)
            {
                value = typedItem;
                return true;
            }

            value = default;
            return false;
        }

        public void Set<T>(string key, T value, TimeSpan expiration)
        {
            if (value is not null)
            {
                _cache[key] = value;
            }
        }

        public void Remove(string key) => _cache.Remove(key);
    }
}
