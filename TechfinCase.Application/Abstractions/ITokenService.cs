namespace TechfinCase.Application.Abstractions;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) Generate(Guid userId, string email);
}
