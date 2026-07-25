using System.Text;
using System.Text.Json;
using Orders.API.Messages;
using Orders.API.Repositories.Interfaces;
using Orders.API.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Orders.API.Messages;
using Orders.API.Repositories.Interfaces;

namespace Orders.API.Consumers;

public class StockReservedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StockReservedConsumer> _logger;

    public StockReservedConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<StockReservedConsumer> logger)
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


        var connection = await factory.CreateConnectionAsync();

        var channel = await connection.CreateChannelAsync();


        await channel.QueueDeclareAsync(
            queue: QueueNames.StockReserved,
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
                    JsonSerializer.Deserialize<StockReservedMessage>(json);


                if(message == null)
                    return;


                using var scope = _scopeFactory.CreateScope();


                var repository =
                    scope.ServiceProvider
                        .GetRequiredService<IOrderRepository>();

                var pedido =
                    await repository.GetByIdAsync(message.OrderId);


                if(pedido != null)
                {
                    pedido.Estado = "Confirmed";
                    await repository.UpdAsync(pedido);
                }


                await channel.BasicAckAsync(
                    args.DeliveryTag,
                    false
                );
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error procesando StockReserved"
                );

                await channel.BasicNackAsync(
                    args.DeliveryTag,
                    false,
                    true
                );
            }
        };


        await channel.BasicConsumeAsync(
            queue: QueueNames.StockReserved,
            autoAck: false,
            consumer: consumer
        );


        await Task.Delay(
            Timeout.Infinite,
            stoppingToken
        );
    }
}