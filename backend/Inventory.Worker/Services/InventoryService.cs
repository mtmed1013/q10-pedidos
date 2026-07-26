using Inventory.Worker.Entities;
using Inventory.Worker.Messages;
using Inventory.Worker.Messaging;
using Inventory.Worker.Messaging.Interfaces;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Services.Interfaces;

namespace Inventory.Worker.Services;

public class InventoryService : IInventoryService
{
    private readonly IStockRepository _repository;
    private readonly IInboundOrderRepository _inboundRepository;
    private readonly IMessagePublisher _publisher;

    public InventoryService(
        IStockRepository repository,
        IInboundOrderRepository inboundRepository,
        IMessagePublisher publisher)
    {
        _repository = repository;
        _inboundRepository = inboundRepository;
        _publisher = publisher;
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

    public async Task ProcessOrderCreatedAsync(
    OrderCreatedMessage message)
    {
        InboundOrder? inbound =
            await _inboundRepository.GetByIdAsync(message.EventId);

        if (inbound is null)
        {
            await ProcessOrderAsync(
                message.EventId,
                message.OrderId,
                message.Sku,
                message.Cantidad);

            inbound =
                await _inboundRepository.GetByIdAsync(message.EventId);
        }

        if (inbound?.Estado == "Pending" && inbound.HasStock)
        {
            await ReserveStockAsync(
                inbound.Sku,
                inbound.Cantidad);

            inbound.Estado = "Reserved";
            await _inboundRepository.UpdateAsync(inbound);

            await _publisher.PublishAsync(
                new StockReservedMessage
                {
                    EventId = Guid.NewGuid(),
                    OrderId = message.OrderId,
                    Sku = message.Sku,
                    Cantidad = message.Cantidad,
                    OcurridoEn = DateTime.UtcNow
                },
                QueueNames.StockReserved);
        }
        else if (inbound?.Estado == "Pending")
        {
            inbound.Estado = "Rejected";
            await _inboundRepository.UpdateAsync(inbound);

            await _publisher.PublishAsync(
                new StockRejectedMessage
                {
                    EventId = Guid.NewGuid(),
                    OrderId = message.OrderId,
                    Sku = message.Sku,
                    Cantidad = message.Cantidad,
                    Motivo = "Stock insuficiente",
                    OcurridoEn = DateTime.UtcNow
                },
                QueueNames.StockRejected);
        }
    }
}
