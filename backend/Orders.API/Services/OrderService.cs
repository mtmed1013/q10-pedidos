using System;
using Orders.API.Messages;
using Orders.API.Dtos;
using Orders.API.Entities;
using Orders.API.Helpers;
using Orders.API.Messaging;
using Orders.API.Messaging.Interfaces;
using Orders.API.Repositories.Interfaces;
using Orders.API.Services.interfaces;
using Orders.API.Services.Transforms;
using Orders.API.Services.Validators;

namespace Orders.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly IStockRepository _stockRepository;

    public OrderService(IOrderRepository repository, IMessagePublisher publisher, 
    IStockRepository stockRepository)
    {
        _repository = repository;
        _publisher = publisher;
        _stockRepository = stockRepository;
    }

    public async Task<Pedido> GetByIdAsync(Guid uuid)
    {
        Pedido pedido = await _repository.GetByIdAsync(uuid);
        GeneralValidator.ValidateDataExists(pedido, "Pedido no encontrado");
        return pedido;
    }

    public async Task<IEnumerable<Pedido>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Pedido> AddAsync(CreateOrderDto dto)
    {
        OrderValidator.ValidateAdd(dto);
        Pedido entity = OrderTransform.TransformToEntity(dto);
        Stock stock = await _stockRepository.GetBySkuAsync(dto.Sku);
        StockValidator.ValidateIfExistsBeforeCreated(stock);
        await _repository.AddAsync(entity);
        await _publisher.PublishAsync(
        new OrderCreatedMessage
        {
            EventId = Guid.NewGuid(),
            OrderId = entity.Id,
            Sku = entity.Sku,
            Cantidad = entity.Cantidad,
            OcurridoEn = DateTime.UtcNow
        },
        QueueNames.OrderCreated
    );
        return entity;
    }
}