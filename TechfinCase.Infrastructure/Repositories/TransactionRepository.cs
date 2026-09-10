using Dapper;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Entities;
using TechfinCase.Infrastructure.Database;

namespace TechfinCase.Infrastructure.Repositories;

public sealed class TransactionRepository(DatabaseConnectionFactory connectionFactory)
    : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            INSERT INTO Transactions (Id, ClientId, Amount, CreatedAtUtc)
            VALUES (@Id, @ClientId, @Amount, @CreatedAtUtc);
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = transaction.Id.ToString(),
                ClientId = transaction.ClientId.ToString(),
                transaction.Amount,
                CreatedAtUtc = transaction.CreatedAtUtc.ToString("O")
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
