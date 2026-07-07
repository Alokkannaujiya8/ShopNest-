using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.MobileNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(string.Empty);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.MobileNumber).IsUnique().HasFilter("[MobileNumber] <> ''");
        builder.HasIndex(x => x.CreatedAtUtc);

        // Soft Delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class AppUserRoleConfiguration : IEntityTypeConfiguration<AppUserRole>
{
    public void Configure(EntityTypeBuilder<AppUserRole> builder)
    {
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OtpHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Purpose)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.OtpVerifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Module).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.IPAddress).HasMaxLength(45);
        builder.Property(x => x.Browser).HasMaxLength(100);
        builder.Property(x => x.Device).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.RequestId).HasMaxLength(100);
        builder.Property(x => x.Role).HasMaxLength(50);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuditLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EntityName);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.Module);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
