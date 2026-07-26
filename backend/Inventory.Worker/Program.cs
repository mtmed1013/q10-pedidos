using Inventory.Worker.Consumers;
using Inventory.Worker.Data;
using Inventory.Worker.Messaging;
using Inventory.Worker.Messaging.Interfaces;
using Inventory.Worker.Repositories;
using Inventory.Worker.Repositories.Interfaces;
using Inventory.Worker.Services;
using Inventory.Worker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInboundOrderRepository, InboundOrderRepository>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();

await using (var scope = host.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

host.Run();
