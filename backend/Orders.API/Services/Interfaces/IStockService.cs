using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Services.interfaces;

public interface IStockService
{
    Task<IEnumerable<ListBoxDto>> GetListAsync();
}