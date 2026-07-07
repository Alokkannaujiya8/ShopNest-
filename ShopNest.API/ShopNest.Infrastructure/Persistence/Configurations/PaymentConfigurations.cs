using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.ProviderPaymentId)
            .HasMaxLength(100);

        builder.Property(x => x.ProviderOrderId)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Order)
            .WithOne(x => x.Payment)
            .HasForeignKey<Payment>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderId).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GatewayTransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.GatewayTransactionId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Reason)
            .HasMaxLength(256);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasOne(x => x.Payment)
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.TransactionId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
