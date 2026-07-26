using System.Text;
using System.Text.Json;
using Inventory.Worker.Messages;
using Inventory.Worker.Messaging;
using Inventory.Worker.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventory.Worker.Consumers;

public class OrderCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderCreatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"]
                ?? throw new InvalidOperationException("RabbitMQ:UserName no configurado"),
            Password = _configuration["RabbitMQ:Password"]
                ?? throw new InvalidOperationException("RabbitMQ:Password no configurado")
        };

        IConnection? connection = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error connecting to RabbitMQ, retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (connection is null)
            return;

        using var activeConnection = connection;
        using var channel = await activeConnection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueNames.OrderCreated,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<OrderCreatedMessage>(json);

                if (message is null)
                {
                    await channel.BasicNackAsync(args.DeliveryTag, false, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var inventoryService = scope.ServiceProvider
                    .GetRequiredService<IInventoryService>();

                await inventoryService.ProcessOrderCreatedAsync(message);

                await channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando OrderCreated");
                await channel.BasicNackAsync(args.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.OrderCreated,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
