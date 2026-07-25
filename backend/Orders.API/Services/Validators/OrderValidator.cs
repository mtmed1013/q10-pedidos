using System;
using Orders.API.Dtos;
using Orders.API.Exceptions;

namespace Orders.API.Services.Validators
{
    public class OrderValidator
    {
        public static void ValidateAdd(CreateOrderDto dto)
        {
            if (dto == null)
                throw new CustomException(400, "El pedido no puede estar vacío");
            
            if(string.IsNullOrWhiteSpace(dto.ClienteNombre))
                throw new CustomException(400, "El nombre del cliente es requerido.");
            
            if(string.IsNullOrWhiteSpace(dto.Sku))
                throw new CustomException(400, "El Sku es requerido.");
        }
    }
}