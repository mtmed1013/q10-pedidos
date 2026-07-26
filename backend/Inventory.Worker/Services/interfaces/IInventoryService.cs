using Inventory.Worker.Messages;

namespace Inventory.Worker.Services.Interfaces;

public interface IInventoryService
{
    Task ProcessOrderCreatedAsync(OrderCreatedMessage message);
    Task<bool> ProcessOrderAsync(Guid eventId, Guid orderId, string sku, int cantidad);
    Task ReserveStockAsync(string sku, int cantidad);
}
