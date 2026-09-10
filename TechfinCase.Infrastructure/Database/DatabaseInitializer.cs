using Dapper;

namespace TechfinCase.Infrastructure.Database;

public sealed class DatabaseInitializer(DatabaseConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Create();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Clients (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Cpf TEXT NOT NULL UNIQUE,
                CreditLimit NUMERIC NOT NULL CHECK (CreditLimit >= 0)
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                ClientId TEXT NOT NULL,
                Amount NUMERIC NOT NULL CHECK (Amount > 0),
                CreatedAtUtc TEXT NOT NULL,
                FOREIGN KEY (ClientId) REFERENCES Clients(Id)
            );

            CREATE TABLE IF NOT EXISTS ProcessedMessages (
                MessageId TEXT PRIMARY KEY,
                ProcessedAtUtc TEXT NOT NULL
            );
            """;

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
}
