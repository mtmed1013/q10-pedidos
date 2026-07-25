using System;

namespace Orders.API.Dtos;

public class CreateOrderDto
{
    public string ClienteNombre { get; set; }
    public string Sku { get; set; }
    public int Cantidad { get; set; }
}