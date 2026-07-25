using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Worker.Entities;

public class Stock
{
    [Key]
    public string Sku { get; set; }
    public int Disponible { get; set; }
}
