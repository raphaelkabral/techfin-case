using Dapper;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Entities;
using TechfinCase.Infrastructure.Database;

namespace TechfinCase.Infrastructure.Repositories;

public sealed class ClientRepository(DatabaseConnectionFactory connectionFactory) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            SELECT Id, Name, Cpf, CreditLimit
            FROM Clients
            WHERE Id = @Id;
            """;

        var command = new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Client>(command);
    }

    public async Task<Client?> GetByCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            SELECT Id, Name, Cpf, CreditLimit
            FROM Clients
            WHERE Cpf = @Cpf;
            """;

        var command = new CommandDefinition(
            sql,
            new { Cpf = cpf },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Client>(command);
    }

    public async Task<IReadOnlyList<Client>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            SELECT Id, Name, Cpf, CreditLimit
            FROM Clients
            ORDER BY Name;
            """;

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var clients = await connection.QueryAsync<Client>(command);

        return clients.ToList();
    }

    public async Task AddAsync(
        Client client,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            INSERT INTO Clients (Id, Name, Cpf, CreditLimit)
            VALUES (@Id, @Name, @Cpf, @CreditLimit);
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = client.Id.ToString(),
                client.Name,
                client.Cpf,
                client.CreditLimit
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<bool> DebitLimitAsync(
        Guid clientId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            UPDATE Clients
            SET CreditLimit = CreditLimit - @Amount
            WHERE Id = @Id
              AND CreditLimit >= @Amount;
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = clientId.ToString(),
                Amount = amount
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteAsync(command) == 1;
    }
}
