using System;
using Microsoft.EntityFrameworkCore;
using Orders.API.Data;
using Orders.API.Dtos;
using Orders.API.Entities;
using Orders.API.Repositories.Interfaces;

namespace Orders.API.Repositories;

public class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    public StockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ListBoxDto>> GetListAsync()
    {
        return await _context.Stock
        .AsNoTracking()
        .Select(x => new ListBoxDto
        {
            Id = x.Sku,
            Label = x.Sku
        })
        .ToListAsync();
    }

}