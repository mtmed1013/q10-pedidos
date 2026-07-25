namespace Inventory.Worker.Messages;
public class StockRejectedMessage
{
    public Guid EventId { get; set; }

    public Guid OrderId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public DateTime OcurridoEn { get; set; }
}