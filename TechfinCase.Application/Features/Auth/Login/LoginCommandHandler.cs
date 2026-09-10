using MediatR;
using TechfinCase.Application.Abstractions;

namespace TechfinCase.Application.Features.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> Handle(        LoginCommand request,        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return null;
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Senha, user.PasswordHash))
        {
            return null;
        }

        var token = tokenService.Generate(user.Id, user.Email);

        return new LoginResponse(token.Token, token.ExpiresAtUtc);
    }
}
