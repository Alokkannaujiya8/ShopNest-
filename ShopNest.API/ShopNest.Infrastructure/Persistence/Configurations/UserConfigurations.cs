using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Gender)
            .HasMaxLength(32);

        builder.Property(x => x.Bio)
            .HasMaxLength(500);

        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(512);

        builder.Property(x => x.ProfilePicturePublicId)
            .HasMaxLength(128);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithOne(x => x.Profile)
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
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

        builder.Property(x => x.AddressType)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.NotificationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.RelatedEntity)
            .HasMaxLength(50);

        builder.Property(x => x.RelatedEntityId)
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsRead);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
