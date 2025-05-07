using EllipticCurve.Utils;
using Microsoft.EntityFrameworkCore;

namespace OnePageCheckoutPackage.Models;

public class CheckoutDbContext : DbContext
{
    public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<BillingDetails> BillingDetails { get; set; }
    public DbSet<ShippingDetails> ShippingDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure precision and scale for decimal properties
        modelBuilder.Entity<CartItem>()
            .Property(c => c.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");

        base.OnModelCreating(modelBuilder);
    }
}