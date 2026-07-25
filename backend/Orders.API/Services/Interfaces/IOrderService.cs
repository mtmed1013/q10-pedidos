using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Services.interfaces;

public interface IOrderService
{
    Task<Pedido> GetByIdAsync(Guid id);
    Task<IEnumerable<Pedido>> GetAllAsync();
    Task<Pedido> AddAsync(CreateOrderDto dto);
}