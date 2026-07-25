namespace Inventory.Worker.Messaging.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string queueName);
}