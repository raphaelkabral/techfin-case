using MediatR;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Entities;

namespace TechfinCase.Application.Features.Auth.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return new RegisterUserResult(false, "E-mail e senha são obrigatórios.");
        }

        var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            return new RegisterUserResult(false, "Usuário já cadastrado.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Senha)
        };

        await userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResult(true);
    }
}
