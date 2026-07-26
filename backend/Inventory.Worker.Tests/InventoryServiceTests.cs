using Inventory.Worker.Messaging.Interfaces;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Services;
using Moq;
using Inventory.Worker.Entities;
using Inventory.Worker.Messages;
using Inventory.Worker.Messaging;

namespace Inventory.Worker.Tests;

public class InventoryServiceTests
{
    private readonly Mock<IStockRepository> _stockRepositoryMock;
    private readonly Mock<IInboundOrderRepository> _inboundRepositoryMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _stockRepositoryMock = new Mock<IStockRepository>();
        _inboundRepositoryMock =
            new Mock<IInboundOrderRepository>();
        _publisherMock = new Mock<IMessagePublisher>();

        _service = new InventoryService(
            _stockRepositoryMock.Object,
            _inboundRepositoryMock.Object,
            _publisherMock.Object);
    }


    [Fact]
    public async Task ProcessSameEventTwice_ShouldDiscountStockOnlyOnce()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();

        Stock stock = new()
        {
            Sku = "SKU001",
            Disponible = 10
        };

        OrderCreatedMessage message = new()
        {
            EventId = eventId,
            OrderId = orderId,
            Sku = stock.Sku,
            Cantidad = 2,
            OcurridoEn = DateTime.UtcNow
        };

        InboundOrder? savedInbound = null;

        _inboundRepositoryMock
            .Setup(repository =>
                            repository.GetByIdAsync(eventId))
                            .ReturnsAsync(() => savedInbound);

        _inboundRepositoryMock
                    .Setup(repository =>
                            repository.AddAsync(It.IsAny<InboundOrder>()))
                                .Callback<InboundOrder>(inbound =>
                                    savedInbound = inbound)
                            .Returns(Task.CompletedTask);

        _inboundRepositoryMock
            .Setup(repository =>
                repository.UpdateAsync(It.IsAny<InboundOrder>()))
            .Callback<InboundOrder>(inbound =>
                savedInbound = inbound)
            .Returns(Task.CompletedTask);

        _stockRepositoryMock
                .Setup(repository =>
                    repository.GetBySkuAsync(message.Sku))
                .ReturnsAsync(stock);

        _stockRepositoryMock
            .Setup(repository =>
                repository.UpdateAsync(It.IsAny<Stock>()))
            .Returns(Task.CompletedTask);

        _publisherMock
            .Setup(publisher =>
                publisher.PublishAsync(
                    It.IsAny<StockReservedMessage>(),
                    QueueNames.StockReserved))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessOrderCreatedAsync(message);
        await _service.ProcessOrderCreatedAsync(message);

        // Assert
        Assert.Equal(8, stock.Disponible);

        Assert.NotNull(savedInbound);
        Assert.Equal("Reserved", savedInbound.Estado);

        _stockRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(It.IsAny<Stock>()),
            Times.Once);

        _inboundRepositoryMock.Verify(
            repository =>
                repository.AddAsync(It.IsAny<InboundOrder>()),
            Times.Once);

        _inboundRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(It.IsAny<InboundOrder>()),
            Times.Once);

        _publisherMock.Verify(
            publisher =>
                publisher.PublishAsync(
                    It.IsAny<StockReservedMessage>(),
                    QueueNames.StockReserved),
            Times.Once);
    }

    [Fact]
    public async Task ProcessOrderCreated_WithInsufficientStock_ShouldRejectWithoutDiscount()
    {
        // Arrange
        Guid eventId = Guid.NewGuid();
        Guid orderId = Guid.NewGuid();

        Stock stock = new()
        {
            Sku = "SKU002",
            Disponible = 1
        };

        OrderCreatedMessage message = new()
        {
            EventId = eventId,
            OrderId = orderId,
            Sku = stock.Sku,
            Cantidad = 2,
            OcurridoEn = DateTime.UtcNow
        };

        InboundOrder? savedInbound = null;

        _inboundRepositoryMock
            .Setup(repository =>
                            repository.GetByIdAsync(eventId))
                            .ReturnsAsync(() => savedInbound);

        _inboundRepositoryMock
                    .Setup(repository =>
                            repository.AddAsync(It.IsAny<InboundOrder>()))
                                .Callback<InboundOrder>(inbound =>
                                    savedInbound = inbound)
                            .Returns(Task.CompletedTask);

        _inboundRepositoryMock
            .Setup(repository =>
                repository.UpdateAsync(It.IsAny<InboundOrder>()))
            .Callback<InboundOrder>(inbound =>
                savedInbound = inbound)
            .Returns(Task.CompletedTask);

        _stockRepositoryMock
                .Setup(repository =>
                    repository.GetBySkuAsync(message.Sku))
                .ReturnsAsync(stock);


        _publisherMock
            .Setup(publisher =>
                publisher.PublishAsync(
                    It.IsAny<StockRejectedMessage>(),
                    QueueNames.StockRejected))
            .Returns(Task.CompletedTask);

        await _service.ProcessOrderCreatedAsync(message);

        // Assert
        Assert.Equal(1, stock.Disponible);

        Assert.NotNull(savedInbound);
        Assert.Equal("Rejected", savedInbound.Estado);

        _inboundRepositoryMock.Verify(
            repository =>
                repository.AddAsync(It.IsAny<InboundOrder>()),
            Times.Once);

        _inboundRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(It.IsAny<InboundOrder>()),
            Times.Once);

        _publisherMock.Verify(
            publisher =>
                publisher.PublishAsync(
                    It.IsAny<StockRejectedMessage>(),
                    QueueNames.StockRejected),
            Times.Once);
    }
}