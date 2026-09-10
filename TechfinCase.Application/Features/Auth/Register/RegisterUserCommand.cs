using MediatR;

namespace TechfinCase.Application.Features.Auth.Register;

public sealed record RegisterUserCommand(string Email, string Senha) : IRequest<RegisterUserResult>;

public sealed record RegisterUserResult(bool Success, string? Error = null);
