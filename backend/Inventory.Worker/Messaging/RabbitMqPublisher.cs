using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Inventory.Worker.Messaging.Interfaces;

namespace Inventory.Worker.Messaging;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConfiguration _configuration;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task PublishAsync<T>(T message, string queueName)
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

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        string json = JsonSerializer.Serialize(message);
        byte[] body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            body: body);
    }
}
