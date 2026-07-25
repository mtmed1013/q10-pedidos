using Inventory.Worker.Entities;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Services.Interfaces;

namespace Inventory.Worker.Services;

public class InventoryService : IInventoryService
{
    private readonly IStockRepository _repository;

    public InventoryService(IStockRepository repository)
    {
        _repository = repository;

    }

    public async Task<bool> ProcessOrderAsync(Guid orderId, string sku, int cantidad)
    {
        Stock stock = await _repository.GetBySkuAsync(sku);
        bool hasStock = InventoryValidator.ValidateStock(stock, cantidad);
        if (!hasStock)
            return false;
            
        stock.Disponible -= cantidad;
        await _repository.UpdateAsync(stock);
        return true;
    }
}