namespace Orders.API.Messages;

public class StockReservedMessage
{
    public Guid EventId { get; set; }

    public Guid OrderId { get; set; }

    public string Sku { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public DateTime OcurridoEn { get; set; }
}