using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Auth.Login;
using TechfinCase.Domain.Entities;
using Xunit;

namespace TechfinCase.Tests;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var users = new Mock<IUserRepository>();
        users
            .Setup(x => x.GetByEmailAsync("a@a.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Email = "a@a.com",
                PasswordHash = "hash"
            });

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Verify("123456", "hash")).Returns(true);

        var expiresAt = DateTime.UtcNow.AddHours(1);
        var tokenService = new Mock<ITokenService>();
        tokenService
            .Setup(x => x.Generate(It.IsAny<Guid>(), "a@a.com"))
            .Returns(("jwt", expiresAt));

        var handler = new LoginCommandHandler(
            users.Object,
            hasher.Object,
            tokenService.Object);

        var result = await handler.Handle(
            new LoginCommand("a@a.com", "123456"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("jwt", result.Token);
        Assert.Equal(expiresAt, result.ExpiraEmUtc);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        var users = new Mock<IUserRepository>();
        users
            .Setup(x => x.GetByEmailAsync("a@a.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                Email = "a@a.com",
                PasswordHash = "hash"
            });

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Verify("wrong", "hash")).Returns(false);

        var handler = new LoginCommandHandler(
            users.Object,
            hasher.Object,
            new Mock<ITokenService>().Object);

        var result = await handler.Handle(
            new LoginCommand("a@a.com", "wrong"),
            CancellationToken.None);

        Assert.Null(result);
    }
}
