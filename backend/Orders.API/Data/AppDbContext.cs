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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stock>().HasData(
            new Stock
            {
                Sku = "SKU001",
                Disponible = 10
            },
            new Stock
            {
                Sku = "SKU002",
                Disponible = 5
            },
            new Stock
            {
                Sku = "SKU003",
                Disponible = 20
            },
            new Stock
            {
                Sku = "SKU004",
                Disponible = 40
            },
            new Stock
            {
                Sku = "SKU005",
                Disponible = 2
            }
        );
    }
}