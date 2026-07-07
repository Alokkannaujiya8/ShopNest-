using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DiscountType)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.MinOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.MaxDiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Usages)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.CouponUsages)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Order)
            .WithOne(x => x.CouponUsage)
            .HasForeignKey<CouponUsage>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.CouponId });
        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
