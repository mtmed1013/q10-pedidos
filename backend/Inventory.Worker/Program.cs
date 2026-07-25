using Inventory.Worker;
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
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

// builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OrderCreatedConsumer>();

var host = builder.Build();
host.Run();