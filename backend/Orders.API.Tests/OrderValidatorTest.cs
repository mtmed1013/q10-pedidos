using Orders.API.Dtos;
using Orders.API.Exceptions;
using Orders.API.Services.Validators;

namespace Orders.API.Tests;

public class OrderValidatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void ValidateAdd_WithInvalidQuantity_ShouldThrowCustomException(
        int cantidad)
    {
        // Arrange
        CreateOrderDto dto = new()
        {
            ClienteNombre = "Mateo",
            Sku = "SKU001",
            Cantidad = cantidad
        };

        // Act
        CustomException exception =
            Assert.Throws<CustomException>(() =>
                OrderValidator.ValidateAdd(dto));

        // Assert
        Assert.Equal(400, exception.Code);
    }

    [Fact]
    public void ValidateAdd_WithBoundaryQuantities_ShouldNotThrowException()
    {
        // Arrange
        CreateOrderDto minimumQuantityDto = new()
        {
            ClienteNombre = "Mateo",
            Sku = "SKU001",
            Cantidad = 1
        };

        CreateOrderDto maximumQuantityDto = new()
        {
            ClienteNombre = "Mateo",
            Sku = "SKU001",
            Cantidad = 100
        };

        // Act
        Exception? minimumException =
            Record.Exception(() =>
                OrderValidator.ValidateAdd(minimumQuantityDto));

        Exception? maximumException =
            Record.Exception(() =>
                OrderValidator.ValidateAdd(maximumQuantityDto));

        // Assert
        Assert.Null(minimumException);
        Assert.Null(maximumException);
    }
}