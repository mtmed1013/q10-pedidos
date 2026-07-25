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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateOrderDto dto)
        {
            Pedido order = await _service.AddAsync(dto);

            return Ok(
                new ApiResponse<Pedido>(
                    true,
                    "Pedido creado correctamente",
                    order
                )
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
        {
            Pedido order = await _service.GetByIdAsync(id);

            return Ok(
                new ApiResponse<Pedido>(
                    true,
                    "Pedido consultado correctamente",
                    order
                )
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            IEnumerable<Pedido> orders = await _service.GetAllAsync();

            return Ok(
                new ApiResponse<IEnumerable<Pedido>>(
                    true,
                    "Pedidos consultados correctamente",
                    orders
                )
            );
        }
    }
}