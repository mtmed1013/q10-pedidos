using System;
using Orders.API.Dtos;
using Orders.API.Entities;
using Orders.API.Helpers;
using Orders.API.Repositories.interfaces;
using Orders.API.Services.interfaces;
using Orders.API.Services.Transforms;
using Orders.API.Services.Validators;

namespace Orders.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    
    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Pedido> GetByIdAsync(Guid uuid)
    {
        Pedido pedido =  await _repository.GetByIdAsync(uuid);
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
        return await _repository.AddAsync(entity);
    }
}