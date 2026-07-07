using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ShippingAddress)
            .HasMaxLength(500);

        builder.Property(x => x.BillingAddress)
            .HasMaxLength(500);

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(x => x.CourierPartner)
            .HasMaxLength(100);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ShippingCost)
            .HasPrecision(18, 2);

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2);

        builder.Property(x => x.Discount)
            .HasPrecision(18, 2);

        builder.Property(x => x.OrderNotes)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.Discount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2);

        builder.Property(x => x.Total)
            .HasPrecision(18, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OrderShippingAddressConfiguration : IEntityTypeConfiguration<OrderShippingAddress>
{
    public void Configure(EntityTypeBuilder<OrderShippingAddress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AlternateMobile)
            .HasMaxLength(20);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Area)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Landmark)
            .HasMaxLength(150);

        builder.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.AddressLine1)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.AddressLine2)
            .HasMaxLength(250);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Order)
            .WithOne(x => x.ShippingAddressDetails)
            .HasForeignKey<OrderShippingAddress>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(100);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OrderTrackingConfiguration : IEntityTypeConfiguration<OrderTracking>
{
    public void Configure(EntityTypeBuilder<OrderTracking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CourierPartner)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.TrackingUpdates)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ReturnRequestConfiguration : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.AdminNotes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.ReturnRequests)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
