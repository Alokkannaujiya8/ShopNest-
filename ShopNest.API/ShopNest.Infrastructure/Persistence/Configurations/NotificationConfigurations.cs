using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(int.MaxValue);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Recipient)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Notification)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
