using Microsoft.EntityFrameworkCore;
using Orders.API.Data;
using Orders.API.Middleware;
using Orders.API.Repositories;
using Orders.API.Repositories.Interfaces;
using Orders.API.Services;
using Orders.API.Services.interfaces;
using Orders.API.Messaging;
using Orders.API.Messaging.Interfaces;
using Orders.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<StockReservedConsumer>();
builder.Services.AddHostedService<StockRejectedConsumer>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.MapControllers();

app.Run();