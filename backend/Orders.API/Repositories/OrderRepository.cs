using System;
using Microsoft.EntityFrameworkCore;
using Orders.API.Data;
using Orders.API.Entities;
using Orders.API.Repositories.interfaces;

namespace Orders.API.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pedido?> GetByIdAsync(Guid id)
    {
        return await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Pedido>> GetAllAsync()
    {
        return await _context.Pedidos.ToListAsync();
    }

    public async Task<Pedido> AddAsync(Pedido entity)
    {
        await _context.Pedidos.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}