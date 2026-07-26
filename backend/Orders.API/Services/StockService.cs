using Orders.API.Dtos;
using Orders.API.Repositories.Interfaces;
using Orders.API.Services.interfaces;

namespace Orders.API.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _stockRepository;

    public StockService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<ListBoxDto>> GetListAsync()
    {
        var list =  await _stockRepository.GetListAsync();
        return list;
    }
}