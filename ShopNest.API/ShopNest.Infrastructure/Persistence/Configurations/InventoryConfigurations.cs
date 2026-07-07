using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UnitCost)
            .HasPrecision(18, 2);

        builder.Property(x => x.SellingPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.LastPurchasePrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Inventories)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique index per product + variant + warehouse (since warehouse can be nullable, we handle it)
        builder.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.WarehouseId }).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TransactionType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.PerformedBy)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Inventory)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.InventoryId);
        builder.HasIndex(x => x.TransactionType);
        builder.HasIndex(x => x.TransactionNumber).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
