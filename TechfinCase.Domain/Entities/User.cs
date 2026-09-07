namespace TechfinCase.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
}
