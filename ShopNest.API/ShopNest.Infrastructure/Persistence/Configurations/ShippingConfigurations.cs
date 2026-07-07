using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Contact)
            .HasMaxLength(50);

        builder.Property(x => x.Website)
            .HasMaxLength(150);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(250);

        builder.Property(x => x.Cost)
            .HasPrecision(18, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShipmentNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ShippingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.BillingAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ShippingCharges)
            .HasPrecision(18, 2);

        builder.Property(x => x.DeliveryInstructions)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Shipments)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Courier)
            .WithMany()
            .HasForeignKey(x => x.CourierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ShipmentTrackingConfiguration : IEntityTypeConfiguration<ShipmentTracking>
{
    public void Configure(EntityTypeBuilder<ShipmentTracking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Shipment)
            .WithMany(x => x.TrackingHistory)
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
