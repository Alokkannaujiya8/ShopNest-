using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;


public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActivityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.IPAddress).HasMaxLength(45);
        builder.Property(x => x.Browser).HasMaxLength(100);
        builder.Property(x => x.Device).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.OS).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.ActivityType);
    }
}

public sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.FailureReason).HasMaxLength(100);
        builder.Property(x => x.IPAddress).HasMaxLength(45);
        builder.Property(x => x.Browser).HasMaxLength(100);
        builder.Property(x => x.Device).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.OS).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.Email);
    }
}

public sealed class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExceptionMessage).IsRequired();
        builder.Property(x => x.ExceptionType).HasMaxLength(250);
        builder.Property(x => x.Source).HasMaxLength(250);
        builder.Property(x => x.RequestPath).HasMaxLength(500);
        builder.Property(x => x.RequestMethod).HasMaxLength(10);
        builder.Property(x => x.UserId).HasMaxLength(100);
        builder.Property(x => x.IPAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.RequestId).HasMaxLength(100);
        builder.Property(x => x.Severity).IsRequired().HasMaxLength(20);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.Severity);
    }
}

public sealed class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLog>
{
    public void Configure(EntityTypeBuilder<ApplicationLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.Level).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.RequestId).HasMaxLength(100);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.Level);
    }
}
