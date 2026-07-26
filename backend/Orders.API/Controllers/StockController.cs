using System;
using Microsoft.AspNetCore.Mvc;
using Orders.API.Dtos;
using Orders.API.Entities;
using Orders.API.Responses;
using Orders.API.Services.interfaces;

namespace Orders.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _service;

        public StockController(IStockService service)
        {
            _service = service;
        }

      


        [HttpGet("list")]
        public async Task<IActionResult> GetAllAsync()
        {
            IEnumerable<ListBoxDto> stock = await _service.GetListAsync();

            return Ok(
                new ApiResponse<IEnumerable<ListBoxDto>>(
                    true,
                    "Stock consultados correctamente",
                    stock
                )
            );
        }
    }
}