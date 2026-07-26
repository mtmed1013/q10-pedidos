using System;
using Orders.API.Dtos;
using Orders.API.Entities;
using Orders.API.Exceptions;

namespace Orders.API.Services.Validators
{
    public class StockValidator
    {
        public static void ValidateIfExistsBeforeCreated(Stock dto)
        {
            if (dto == null)
                throw new CustomException(409, "El stock no existe, por ende no se puede descontar");
        }
    }
}