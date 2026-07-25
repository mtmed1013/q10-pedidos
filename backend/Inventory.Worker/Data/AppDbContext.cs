using Microsoft.EntityFrameworkCore;
using Inventory.Worker.Entities;

namespace Inventory.Worker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Stock> Stock { get; set; }
}