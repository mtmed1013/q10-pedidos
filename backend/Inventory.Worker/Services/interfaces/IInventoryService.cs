namespace Inventory.Worker.Services.Interfaces;

public interface IInventoryService
{
    Task<bool> ProcessOrderAsync(Guid eventId, Guid orderId, string sku, int cantidad);
    Task ReserveStockAsync(string sku, int cantidad);
}
