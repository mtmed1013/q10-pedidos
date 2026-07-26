using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Worker.Entities;

public class InboundOrder
{
    [Key]
    public Guid EventId { get; set; }

    public Guid OrderId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool HasStock { get; set; }
    public DateTime ProcesadoEn { get; set; }
}
