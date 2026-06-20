using Microsoft.EntityFrameworkCore;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;

namespace ShopNest.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ShopNestDbContext context)
    {
        // 1. Seed Users
        if (!await context.Users.AnyAsync())
        {
            var admin = new AppUser
            {
                Id = Guid.Parse("37a3c30a-df62-4217-bfbe-bc872a912bb0"),
                FullName = "ShopNest Admin",
                Email = "admin@shopnest.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                Role = UserRole.Admin,
                CreatedAtUtc = DateTime.UtcNow
            };

            var customer = new AppUser
            {
                Id = Guid.Parse("57f00bc1-7975-4d04-bf76-5991823eb91f"),
                FullName = "John Customer",
                Email = "customer@shopnest.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CustomerPassword123!"),
                Role = UserRole.Customer,
                CreatedAtUtc = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(admin, customer);
            await context.SaveChangesAsync();
        }

        // 2. Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new()
                {
                    Id = Guid.Parse("c07a3c70-659a-41f2-89ea-6b2234033c4a"),
                    Name = "Electronics",
                    Slug = "electronics",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("d07a3c70-659a-41f2-89ea-6b2234033c4b"),
                    Name = "Clothing",
                    Slug = "clothing",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("e07a3c70-659a-41f2-89ea-6b2234033c4c"),
                    Name = "Home & Kitchen",
                    Slug = "home-kitchen",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.Parse("f07a3c70-659a-41f2-89ea-6b2234033c4d"),
                    Name = "Books",
                    Slug = "books",
                    CreatedAtUtc = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // 3. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var electroId = Guid.Parse("c07a3c70-659a-41f2-89ea-6b2234033c4a");
            var clothingId = Guid.Parse("d07a3c70-659a-41f2-89ea-6b2234033c4b");
            var homeId = Guid.Parse("e07a3c70-659a-41f2-89ea-6b2234033c4c");
            var booksId = Guid.Parse("f07a3c70-659a-41f2-89ea-6b2234033c4d");

            var products = new List<Product>
            {
                // Electronics
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = electroId,
                    Name = "Smart OLED TV 55 inch",
                    Slug = "smart-oled-tv-55-inch",
                    Description = "Experience ultra-crisp cinematic visuals with our next-gen OLED smart television featuring AI picture optimization and Dolby Atmos.",
                    Price = 79999.00m,
                    StockQuantity = 15,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = electroId,
                    Name = "Wireless Noise Cancelling Headphones",
                    Slug = "wireless-noise-cancelling-headphones",
                    Description = "Active noise cancelling (ANC) headphones with up to 40 hours of battery life and studio-grade acoustics.",
                    Price = 14999.00m,
                    StockQuantity = 3, // Low stock on purpose!
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = electroId,
                    Name = "Pro Edition Smartphone",
                    Slug = "pro-edition-smartphone",
                    Description = "Equipped with a 108MP camera array, 120Hz dynamic refresh rate screen, and raw flagship speed.",
                    Price = 64999.00m,
                    StockQuantity = 24,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },

                // Clothing
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = clothingId,
                    Name = "Classic Black Leather Jacket",
                    Slug = "classic-black-leather-jacket",
                    Description = "Genuine premium leather jacket tailored for a timeless stylish look and ultimate comfort.",
                    Price = 5999.00m,
                    StockQuantity = 8,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = clothingId,
                    Name = "Ultra-Light Running Sneakers",
                    Slug = "ultra-light-running-sneakers",
                    Description = "Engineered with breathable mesh and responsive foam padding to support long miles.",
                    Price = 3499.00m,
                    StockQuantity = 12,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },

                // Home & Kitchen
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = homeId,
                    Name = "Espresso Coffee Machine",
                    Slug = "espresso-coffee-machine",
                    Description = "15-bar pressure Italian pump coffee maker with adjustable steam nozzle to froth milk for perfect lattes.",
                    Price = 11999.00m,
                    StockQuantity = 10,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1517256064527-09c53b2d0bc6?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = homeId,
                    Name = "Smart Touch Air Fryer",
                    Slug = "smart-touch-air-fryer",
                    Description = "Crispy, healthy fried food using 85% less oil than traditional deep frying with easy touch controls.",
                    Price = 7499.00m,
                    StockQuantity = 18,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1621972750749-0fbb1abb7736?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },

                // Books
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = booksId,
                    Name = "Clean Architecture by Robert C. Martin",
                    Slug = "clean-architecture-by-robert-martin",
                    Description = "A craftsman's guide to software structure and design, presenting rules of architecture and software design patterns.",
                    Price = 799.00m,
                    StockQuantity = 30,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    CategoryId = booksId,
                    Name = "The Pragmatic Programmer",
                    Slug = "the-pragmatic-programmer",
                    Description = "One of the most significant books in software development, detailing best engineering habits and professional practices.",
                    Price = 999.00m,
                    StockQuantity = 2, // Low stock on purpose!
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1629654297299-c8506221ca97?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
