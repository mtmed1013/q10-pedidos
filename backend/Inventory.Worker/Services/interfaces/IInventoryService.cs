namespace Inventory.Worker.Services.Interfaces;

public interface IInventoryService
{
    Task<bool> ProcessOrderAsync(Guid orderId,string sku,int cantidad);
}