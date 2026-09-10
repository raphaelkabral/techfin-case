using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Features.Clients.DebitClientLimit;
using TechfinCase.Domain.Events;

namespace TechfinCase.Infrastructure.Messaging;

public sealed class ClientLimitUpdateConsumer(IServiceScopeFactory scopeFactory, 
                                             IOptions<RabbitMqOptions> options, 
                                             ILogger<ClientLimitUpdateConsumer> logger) : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                StartConsumer();
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "RabbitMQ indisponível. Nova tentativa em 5 segundos.");
                DisposeRabbitResources();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private void StartConsumer()
    {
        var rabbitMqOptions = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: rabbitMqOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;

        _channel.BasicConsume(queue: rabbitMqOptions.QueueName, autoAck: false, consumer: consumer);

        logger.LogInformation("Consumidor RabbitMQ iniciado na fila {QueueName}.", rabbitMqOptions.QueueName);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var message = JsonSerializer.Deserialize<AuthorizedTransactionEvent>(json);

            if (message is null)
            {
                _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                return;
            }

            using var scope = scopeFactory.CreateScope();

            var senderMediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var processedMessageRepository = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();

            var alreadyProcessed = await processedMessageRepository.ExistsAsync(message.TransactionId);

            if (alreadyProcessed)
            {
                logger.LogInformation("Mensagem {TransactionId} já processada.", message.TransactionId);
                _channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            var updated = await senderMediator.Send(new DebitClientLimitCommand(message.ClientId, message.Amount));

            if (updated)
            {
                await processedMessageRepository.AddAsync(message.TransactionId);
                _channel.BasicAck(eventArgs.DeliveryTag, false);
                return;
            }

            _channel.BasicNack(eventArgs.DeliveryTag, false, false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro ao processar atualização de limite do cliente.");
            _channel.BasicNack(eventArgs.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        DisposeRabbitResources();
        base.Dispose();
    }

    private void DisposeRabbitResources()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }
}
