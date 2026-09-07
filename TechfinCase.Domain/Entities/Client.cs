namespace TechfinCase.Domain.Entities;

public sealed class Client
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public decimal CreditLimit { get; set; }
}
