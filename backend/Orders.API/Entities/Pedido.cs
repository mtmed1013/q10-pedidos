using System;

namespace Orders.API.Entities;

public class Pedido
{
    public Guid Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Estado { get; set; } = "Pending";
    public DateTime CreadoEn { get; set; } = DateTime.Now;
}
