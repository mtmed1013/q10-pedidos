using System;
using Inventory.Worker.Entities;

namespace Inventory.Worker.Repositories.Interfaces
{
    public interface IInboundOrderRepository
    {
        Task<InboundOrder?> GetByIdAsync(Guid id);
        Task UpdateAsync(InboundOrder stock);
        Task AddAsync(InboundOrder stock);
    }
}