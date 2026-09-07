using MediatR;

namespace TechfinCase.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Senha) : IRequest<LoginResponse?>;

public sealed record LoginResponse(
    string Token,
    DateTime ExpiraEmUtc);
