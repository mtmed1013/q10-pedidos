using System;
using Microsoft.EntityFrameworkCore;
using Inventory.Worker.Data;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Entities;

namespace Inventory.Worker.Repositories;

public class InboundOrderRepository : IInboundOrderRepository
{
    private readonly AppDbContext _context;

    public InboundOrderRepository (AppDbContext context)
    {
        _context = context;
    }
    public async Task<InboundOrder?> GetByIdAsync(Guid id)
    {
        return await _context.InboundOrder.FirstOrDefaultAsync(s => s.EventId == id);
    }

    public async Task UpdateAsync(InboundOrder entity)
    {
        _context.InboundOrder.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(InboundOrder entity)
    {
        _context.InboundOrder.Add(entity);
        await _context.SaveChangesAsync();
    }


}
