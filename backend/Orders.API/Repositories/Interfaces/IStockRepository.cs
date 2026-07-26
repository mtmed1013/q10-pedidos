using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Repositories.Interfaces;

public interface IStockRepository
{
    Task<IEnumerable<ListBoxDto>> GetListAsync();
}