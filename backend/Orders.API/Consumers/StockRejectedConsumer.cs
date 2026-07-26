using System.Text;
using System.Text.Json;
using Orders.API.Messages;
using Orders.API.Messaging;
using Orders.API.Repositories.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.API.Consumers;

public class StockRejectedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StockRejectedConsumer> _logger;

    public StockRejectedConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<StockRejectedConsumer> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(
                _configuration["RabbitMQ:Port"] ?? "5672"
            )
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

        if (connection == null)
            return;

        var channel = await connection.CreateChannelAsync();


        await channel.QueueDeclareAsync(
            queue: QueueNames.StockRejected,
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
                    JsonSerializer.Deserialize<StockRejectedMessage>(json);


                if (message is null)
                {
                    _logger.LogWarning(
                        "Mensaje StockReserved inválido. Tag: {Tag}",
                        args.DeliveryTag);

                    await channel.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }


                using var scope = _scopeFactory.CreateScope();


                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IOrderRepository>();

                var pedido =
                    await repository.GetByIdAsync(message.OrderId);

                if (pedido is null)
                {
                    _logger.LogWarning(
                        "Pedido {OrderId} no existe para StockReserved",
                        message.OrderId);

                    await channel.BasicNackAsync(
                        args.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }


                if (pedido.Estado == "Pending")
                {
                    pedido.Estado = "Rejected";
                    await repository.UpdAsync(pedido);
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
                    "Error procesando StockRejected"
                );

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    false,
                    true
                );
            }
        };


        await channel.BasicConsumeAsync(
            queue: QueueNames.StockRejected,
            autoAck: false,
            consumer: consumer
        );


        await Task.Delay(
            Timeout.Infinite,
            stoppingToken
        );
    }
}