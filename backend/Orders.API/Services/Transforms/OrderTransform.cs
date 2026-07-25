using System;
using Orders.API.Dtos;
using Orders.API.Entities;

namespace Orders.API.Services.Transforms;

public class OrderTransform
{
    public static Pedido TransformToEntity(CreateOrderDto dto)
    {
        return new Pedido
        {
            Id = Guid.NewGuid(),
            ClienteNombre = dto.ClienteNombre,
            Sku = dto.Sku,
            Cantidad = dto.Cantidad,
            Estado = "Pending",
            CreadoEn = DateTime.Now
        };
    }
}