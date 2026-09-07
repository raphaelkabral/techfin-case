using Dapper;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Entities;
using TechfinCase.Infrastructure.Database;

namespace TechfinCase.Infrastructure.Repositories;

public sealed class UserRepository(DatabaseConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            SELECT Id, Email, PasswordHash
            FROM Users
            WHERE Email = @Email;
            """;

        var command = new CommandDefinition(
            sql,
            new { Email = email },
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(command);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();

        const string sql = """
            INSERT INTO Users (Id, Email, PasswordHash)
            VALUES (@Id, @Email, @PasswordHash);
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = user.Id.ToString(),
                user.Email,
                user.PasswordHash
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}
