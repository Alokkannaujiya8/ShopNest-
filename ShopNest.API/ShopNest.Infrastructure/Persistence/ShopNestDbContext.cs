using Microsoft.EntityFrameworkCore;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence;

public sealed class ShopNestDbContext(DbContextOptions<ShopNestDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.FullName).HasMaxLength(160);
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Slug).HasMaxLength(140);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.Name).HasMaxLength(180);
            entity.Property(x => x.Slug).HasMaxLength(220);
        });

        modelBuilder.Entity<Cart>()
            .HasOne(x => x.User)
            .WithOne(x => x.Cart)
            .HasForeignKey<Cart>(x => x.UserId);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.ProductName).HasMaxLength(180);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Provider).HasMaxLength(40);
            entity.Property(x => x.Currency).HasMaxLength(8);
        });
    }
}
