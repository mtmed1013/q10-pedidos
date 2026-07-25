using System;
using Microsoft.EntityFrameworkCore;
using Inventory.Worker.Data;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Entities;

namespace Inventory.Worker.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockRepository (AppDbContext context)
    {
        _context = context;
    }
    public async Task<Stock?> GetBySkuAsync(string sku)
    {
        return await _context.Stock.FirstOrDefaultAsync(s => s.Sku == sku);
    }

    public async Task UpdateAsync(Stock entity)
    {
        _context.Stock.Update(entity);
        await _context.SaveChangesAsync();
    }


}