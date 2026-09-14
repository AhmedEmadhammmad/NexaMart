using Microsoft.EntityFrameworkCore;
using NexaMart.Domain.Entities;

namespace NexaMart.Infrastructure.Data.Context;

/// <summary>
/// Core database context for NexaMart using custom lightweight identity and entity models.
/// </summary>
public class NexaMartDbContext : DbContext
{
    public NexaMartDbContext(DbContextOptions<NexaMartDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Apply restrictive delete behavior across foreign keys by default to prevent unintended cascade paths
        foreach (var relationship in builder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // 2. Apply Fluent API configurations discovered from this assembly (explicit Cascade configurations take precedence)
        builder.ApplyConfigurationsFromAssembly(typeof(NexaMartDbContext).Assembly);

        // 3. Configure standard decimal(18, 2) precision across all financial and decimal properties
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }
    }
}
