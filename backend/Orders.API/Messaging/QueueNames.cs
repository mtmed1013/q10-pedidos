namespace Orders.API.Messaging;

public static class QueueNames
{
    public const string OrderCreated = "order-created";
    public const string StockReserved = "stock-reserved";
    public const string StockRejected = "stock-rejected";
}