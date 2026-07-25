using System.Text;
using System.Text.Json;
using Inventory.Worker.Messages;
using Inventory.Worker.Messaging;
using Inventory.Worker.Messaging.Interfaces;
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
        ILogger<OrderCreatedConsumer> logger
        )
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
            Port = int.Parse(
                _configuration["RabbitMQ:Port"] ?? "5672"
            )
        };

        var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueNames.OrderCreated,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var message =
                    JsonSerializer.Deserialize<OrderCreatedMessage>(json);

                if (message == null)
                    return;

                _logger.LogInformation(
                    "OrderCreated recibido: {OrderId} - SKU: {Sku} - Cantidad: {Cantidad}",
                    message.OrderId,
                    message.Sku,
                    message.Cantidad
                );

                using var scope = _scopeFactory.CreateScope();

                var inventoryService =
                    scope.ServiceProvider
                        .GetRequiredService<IInventoryService>();



                bool reserved = await inventoryService.ProcessOrderAsync(
                    message.OrderId,
                    message.Sku,
                    message.Cantidad
                );

                var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
                if (reserved)
                {
                    await publisher.PublishAsync(
                        new StockReservedMessage
                        {
                            EventId = Guid.NewGuid(),
                            OrderId = message.OrderId,
                            Sku = message.Sku,
                            Cantidad = message.Cantidad,
                            OcurridoEn = DateTime.UtcNow
                        },
                        QueueNames.StockReserved
                    );
                }
                else
                {
                    await publisher.PublishAsync(
                        new StockRejectedMessage
                        {
                            EventId = Guid.NewGuid(),
                            OrderId = message.OrderId,
                            Sku = message.Sku,
                            Cantidad = message.Cantidad,
                            Motivo = "Stock insuficiente",
                            OcurridoEn = DateTime.UtcNow
                        },
                        QueueNames.StockRejected
                    );
                }

                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    false
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error procesando OrderCreated"
                );

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    false,
                    true
                );
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueNames.OrderCreated,
            autoAck: false,
            consumer: consumer
        );

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken
        );
    }
}