using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Key).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
