using System;
using Inventory.Worker.Entities;

namespace Inventory.Worker.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock?> GetBySkuAsync(string sku);
        Task UpdateAsync(Stock stock);
    }
}