using Microsoft.EntityFrameworkCore;
using Orders.API.Entities;

namespace Orders.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Stock> Stock { get; set; }
}