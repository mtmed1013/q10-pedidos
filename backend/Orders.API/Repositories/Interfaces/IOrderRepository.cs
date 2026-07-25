using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Repositories.interfaces;

public interface IOrderRepository
{
    Task<Pedido> GetByIdAsync(Guid id);
    Task<IEnumerable<Pedido>> GetAllAsync();
    Task<Pedido> AddAsync(Pedido entity);
}