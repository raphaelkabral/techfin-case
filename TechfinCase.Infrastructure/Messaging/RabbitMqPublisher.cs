using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TechfinCase.Application.Abstractions;
using TechfinCase.Domain.Events;

namespace TechfinCase.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    public Task PublishAsync(
        AuthorizedTransactionEvent message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rabbitMqOptions = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: rabbitMqOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: rabbitMqOptions.QueueName,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }
}
