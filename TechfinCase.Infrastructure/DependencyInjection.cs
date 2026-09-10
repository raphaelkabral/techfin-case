using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechfinCase.Application.Abstractions;
using TechfinCase.Infrastructure.Caching;
using TechfinCase.Infrastructure.Database;
using TechfinCase.Infrastructure.Messaging;
using TechfinCase.Infrastructure.Repositories;
using TechfinCase.Infrastructure.Security;

namespace TechfinCase.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(        this IServiceCollection services,        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=TechfinCase;Mode=Memory;Cache=Shared";

        services.AddSingleton(new DatabaseConnectionFactory(connectionString));
        services.AddSingleton<DatabaseInitializer>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();


        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();

        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddHostedService<ClientLimitUpdateConsumer>();

        return services;
    }
}
