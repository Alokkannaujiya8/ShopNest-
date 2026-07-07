using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopNest.Domain.Entities;

namespace ShopNest.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(140);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(250);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(512);

        builder.Property(x => x.ImagePublicId)
            .HasMaxLength(150);

        builder.Property(x => x.BannerUrl)
            .HasMaxLength(512);

        builder.Property(x => x.BannerPublicId)
            .HasMaxLength(150);

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(150);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(250);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(250);

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(140);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(512);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(220);

        builder.Property(x => x.ShortDescription)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.CostPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountType)
            .HasMaxLength(50);

        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.TaxPercentage)
            .HasPrecision(5, 2);

        builder.Property(x => x.StockStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.Property(x => x.MetaTitle)
            .HasMaxLength(150);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(250);

        builder.Property(x => x.MetaKeywords)
            .HasMaxLength(250);

        // Foreign keys - restrict deletes of Category/Brand if products exist
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubCategory)
            .WithMany()
            .HasForeignKey(x => x.SubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.SubCategoryId);
        builder.HasIndex(x => x.BrandId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Sku)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Barcode)
            .HasMaxLength(50);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(512);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => x.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        builder.HasIndex(x => x.ProductId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReviewTitle)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ReviewDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.AdminNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.ReportReason)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProductId);
        builder.HasIndex(x => x.UserId);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Value).IsRequired().HasMaxLength(250);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.AttributeValues)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductAttribute)
            .WithMany(x => x.Values)
            .HasForeignKey(x => x.ProductAttributeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductCategories)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.ProductId, x.CategoryId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
{
    public void Configure(EntityTypeBuilder<ReviewImage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(250);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Review)
            .WithMany(x => x.ReviewImages)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ReviewHelpfulVoteConfiguration : IEntityTypeConfiguration<ReviewHelpfulVote>
{
    public void Configure(EntityTypeBuilder<ReviewHelpfulVote> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Review)
            .WithMany(x => x.HelpfulVotes)
            .HasForeignKey(x => x.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductQuestionConfiguration : IEntityTypeConfiguration<ProductQuestion>
{
    public void Configure(EntityTypeBuilder<ProductQuestion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuestionText).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class ProductAnswerConfiguration : IEntityTypeConfiguration<ProductAnswer>
{
    public void Configure(EntityTypeBuilder<ProductAnswer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AnswerText).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Question)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
