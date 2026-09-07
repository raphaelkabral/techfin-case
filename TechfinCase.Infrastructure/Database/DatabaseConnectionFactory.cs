using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace TechfinCase.Infrastructure.Database;

public sealed class DatabaseConnectionFactory : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _keepAliveConnection;

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    public DbConnection Create() => new SqliteConnection(_connectionString);

    public void Dispose() => _keepAliveConnection.Dispose();
}
