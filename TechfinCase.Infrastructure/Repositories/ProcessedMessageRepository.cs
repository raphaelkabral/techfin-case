using Dapper;
using TechfinCase.Application.Abstractions;
using TechfinCase.Infrastructure.Database;

namespace TechfinCase.Infrastructure.Repositories;

public sealed class ProcessedMessageRepository(DatabaseConnectionFactory connectionFactory) : IProcessedMessageRepository
{
    public async Task<bool> ExistsAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            SELECT COUNT(1)
            FROM ProcessedMessages
            WHERE MessageId = @MessageId;
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                MessageId = messageId.ToString()
            },
            cancellationToken: cancellationToken);

        var count = await connection.ExecuteScalarAsync<int>(command);

        return count > 0;
    }

    public async Task AddAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            INSERT INTO ProcessedMessages (
                MessageId,
                ProcessedAtUtc
            )
            VALUES (
                @MessageId,
                @ProcessedAtUtc
            );
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                MessageId = messageId.ToString(),
                ProcessedAtUtc = DateTime.UtcNow.ToString("O")
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}