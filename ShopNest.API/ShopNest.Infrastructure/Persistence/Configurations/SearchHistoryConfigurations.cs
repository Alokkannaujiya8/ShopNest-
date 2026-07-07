using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
{
    public void Configure(EntityTypeBuilder<SearchHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.QueryText)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.QueryText);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
