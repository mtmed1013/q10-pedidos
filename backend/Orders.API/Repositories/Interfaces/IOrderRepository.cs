using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Pedido> GetByIdAsync(Guid id);
    Task<IEnumerable<Pedido>> GetAllAsync();
    Task<Pedido> AddAsync(Pedido entity);
    Task<Pedido> UpdAsync(Pedido entity);
}