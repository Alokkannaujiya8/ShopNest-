using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class PermissionGroupConfiguration : IEntityTypeConfiguration<PermissionGroup>
{
    public void Configure(EntityTypeBuilder<PermissionGroup> builder)
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

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.PermissionGroup)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.PermissionGroupId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(x => new { x.RoleId, x.PermissionId });

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(x => new { x.UserId, x.PermissionId });

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.UserPermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
