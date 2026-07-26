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
    public DbSet<InboundOrder> InboundOrder { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pedido>()
            .ToTable("Pedidos", table => table.ExcludeFromMigrations());

        modelBuilder.Entity<Stock>()
            .ToTable("Stock", table => table.ExcludeFromMigrations());

        modelBuilder.Entity<InboundOrder>(entity =>
        {
            entity.ToTable("InboundOrder");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.Sku).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Estado).HasMaxLength(20).IsRequired();
        });

    }
}
