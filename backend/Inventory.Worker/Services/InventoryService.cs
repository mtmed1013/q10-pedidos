using Inventory.Worker.Entities;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Services.Interfaces;

namespace Inventory.Worker.Services;

public class InventoryService : IInventoryService
{
    private readonly IStockRepository _repository;
    private readonly IInboundOrderRepository _inboundRepository;

    public InventoryService(
        IStockRepository repository,
        IInboundOrderRepository inboundRepository)
    {
        _repository = repository;
        _inboundRepository = inboundRepository;
    }

    public async Task<bool> ProcessOrderAsync(
        Guid eventId,
        Guid orderId,
        string sku,
        int cantidad)
    {
        Stock stock = await _repository.GetBySkuAsync(sku);
        bool hasStock = InventoryValidator.ValidateStock(stock, cantidad);

        InboundOrder entity = new()
        {
            EventId = eventId,
            OrderId = orderId,
            Sku = sku,
            Cantidad = cantidad,
            Estado = "Pending",
            HasStock = hasStock,
            ProcesadoEn = DateTime.UtcNow
        };

        await _inboundRepository.AddAsync(entity);
        return hasStock;
    }

    public async Task ReserveStockAsync(string sku, int cantidad)
    {
        Stock stock = await _repository.GetBySkuAsync(sku);
        stock.Disponible -= cantidad;
        await _repository.UpdateAsync(stock);
    }
}
