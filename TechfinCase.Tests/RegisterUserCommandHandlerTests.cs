using Moq;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Auth.Register;
using TechfinCase.Domain.Entities;
using Xunit;

namespace TechfinCase.Tests;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRejectExistingEmail()
    {
        var users = new Mock<IUserRepository>();
        users
            .Setup(x => x.GetByEmailAsync("a@a.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Email = "a@a.com" });

        var handler = new RegisterUserCommandHandler(
            users.Object,
            new Mock<IPasswordHasher>().Object);

        var result = await handler.Handle(
            new RegisterUserCommand("a@a.com", "123456"),
            CancellationToken.None);

        Assert.False(result.Success);
        users.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenRequestIsValid()
    {
        var users = new Mock<IUserRepository>();
        users
            .Setup(x => x.GetByEmailAsync("a@a.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(x => x.Hash("123456")).Returns("hash");

        var handler = new RegisterUserCommandHandler(users.Object, hasher.Object);

        var result = await handler.Handle(
            new RegisterUserCommand("A@A.COM", "123456"),
            CancellationToken.None);

        Assert.True(result.Success);
        users.Verify(
            x => x.AddAsync(
                It.Is<User>(u => u.Email == "a@a.com" && u.PasswordHash == "hash"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
