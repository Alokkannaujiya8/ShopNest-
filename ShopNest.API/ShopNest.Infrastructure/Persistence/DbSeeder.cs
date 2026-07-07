using Microsoft.EntityFrameworkCore;
using ShopNest.Domain.Entities;
using ShopNest.Domain.Enums;

namespace ShopNest.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(ShopNestDbContext context)
    {
        var superAdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var sellerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // 1. Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            await context.Roles.AddRangeAsync(
                new Role { Id = superAdminRoleId, Name = "SuperAdmin", DisplayName = "Super Admin", Description = "Super Administrator role" },
                new Role { Id = adminRoleId, Name = "Admin", DisplayName = "Admin", Description = "Administrator role" },
                new Role { Id = customerRoleId, Name = "Customer", DisplayName = "Customer", Description = "Customer role" },
                new Role { Id = sellerRoleId, Name = "Seller", DisplayName = "Seller", Description = "Seller role" }
            );
            await context.SaveChangesAsync();
        }

        // 1.2 Seed Permissions
        if (!await context.PermissionGroups.AnyAsync())
        {
            var userGroup = new PermissionGroup { Id = Guid.NewGuid(), Name = "User Management", Description = "Manage users and locks" };
            var roleGroup = new PermissionGroup { Id = Guid.NewGuid(), Name = "Role Management", Description = "Manage roles and assignments" };
            
            await context.PermissionGroups.AddRangeAsync(userGroup, roleGroup);
            await context.SaveChangesAsync();

            var permissions = new List<Permission>
            {
                new() { Id = Guid.NewGuid(), PermissionGroupId = userGroup.Id, Name = "Users.View", DisplayName = "View Users", Description = "Allows viewing lists of users" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = userGroup.Id, Name = "Users.Create", DisplayName = "Create Users", Description = "Allows creating new users" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = userGroup.Id, Name = "Users.Update", DisplayName = "Update Users", Description = "Allows modifying users" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = userGroup.Id, Name = "Users.Delete", DisplayName = "Delete Users", Description = "Allows deleting users" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = roleGroup.Id, Name = "Roles.View", DisplayName = "View Roles", Description = "Allows viewing system roles" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = roleGroup.Id, Name = "Roles.Create", DisplayName = "Create Roles", Description = "Allows creating system roles" },
                new() { Id = Guid.NewGuid(), PermissionGroupId = roleGroup.Id, Name = "Roles.Update", DisplayName = "Update Roles", Description = "Allows updating system roles" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();

            // Link all to Admin role and Super Admin role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin") ?? throw new InvalidOperationException("Admin role not found.");
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin") ?? throw new InvalidOperationException("SuperAdmin role not found.");

            var adminPermissions = permissions.Select(p => new RolePermission { Role = adminRole, PermissionId = p.Id });
            var superPermissions = permissions.Select(p => new RolePermission { Role = superAdminRole, PermissionId = p.Id });
            await context.RolePermissions.AddRangeAsync(adminPermissions);
            await context.RolePermissions.AddRangeAsync(superPermissions);
            await context.SaveChangesAsync();
        }

        // 2. Seed Users
        var superAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var adminId = Guid.Parse("37a3c30a-df62-4217-bfbe-bc872a912bb0");
        var customerId = Guid.Parse("57f00bc1-7975-4d04-bf76-5991823eb91f");

        if (!await context.Users.AnyAsync(u => u.Email == "superadmin@shopnest.com"))
        {
            var superAdmin = new AppUser
            {
                Id = superAdminId,
                FullName = "ShopNest Super Admin",
                Email = "superadmin@shopnest.com",
                MobileNumber = "+0000000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdminPassword123!"),
                Role = UserRole.Admin,
                IsEmailVerified = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            await context.Users.AddAsync(superAdmin);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(u => u.Email == "admin@shopnest.com"))
        {
            var admin = new AppUser
            {
                Id = adminId,
                FullName = "ShopNest Admin",
                Email = "admin@shopnest.com",
                MobileNumber = "+1111111111",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                Role = UserRole.Admin,
                IsEmailVerified = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(u => u.Email == "customer@shopnest.com"))
        {
            var customer = new AppUser
            {
                Id = customerId,
                FullName = "John Customer",
                Email = "customer@shopnest.com",
                MobileNumber = "+2222222222",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CustomerPassword123!"),
                Role = UserRole.Customer,
                IsEmailVerified = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            await context.Users.AddAsync(customer);
            await context.SaveChangesAsync();
        }

        // 3. Seed UserRoles mapping
        if (!await context.UserRoles.AnyAsync())
        {
            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin") ?? throw new InvalidOperationException("SuperAdmin role not found.");
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin") ?? throw new InvalidOperationException("Admin role not found.");
            var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer") ?? throw new InvalidOperationException("Customer role not found.");

            var superAdminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@shopnest.com");
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@shopnest.com");
            var customerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer@shopnest.com");

            if (superAdminUser != null)
                await context.UserRoles.AddAsync(new AppUserRole { User = superAdminUser, Role = superAdminRole });
            if (adminUser != null)
                await context.UserRoles.AddAsync(new AppUserRole { User = adminUser, Role = adminRole });
            if (customerUser != null)
                await context.UserRoles.AddAsync(new AppUserRole { User = customerUser, Role = customerRole });

            await context.SaveChangesAsync();
        }

        // 4. Seed Brands
        var appleBrandId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
        var samsungBrandId = Guid.Parse("bbbb2222-2222-2222-2222-222222222222");
        var nikeBrandId = Guid.Parse("cccc3333-3333-3333-3333-333333333333");
        var addyBrandId = Guid.Parse("dddd4444-4444-4444-4444-444444444444");
        var genericBrandId = Guid.Parse("eeee5555-5555-5555-5555-555555555555");

        if (!await context.Brands.AnyAsync())
        {
            await context.Brands.AddRangeAsync(
                new Brand { Id = appleBrandId, Name = "Apple", Slug = "apple", Description = "Apple Inc. electronics" },
                new Brand { Id = samsungBrandId, Name = "Samsung", Slug = "samsung", Description = "Samsung Electronics" },
                new Brand { Id = nikeBrandId, Name = "Nike", Slug = "nike", Description = "Nike sportswear" },
                new Brand { Id = addyBrandId, Name = "Adidas", Slug = "adidas", Description = "Adidas sportswear" },
                new Brand { Id = genericBrandId, Name = "Generic", Slug = "generic", Description = "Generic products" }
            );
            await context.SaveChangesAsync();
        }

        // 5. Seed Categories
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

        // 6. Seed Products
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
                    BrandId = samsungBrandId,
                    Name = "Smart OLED TV 55 inch",
                    Slug = "smart-oled-tv-55-inch",
                    Sku = "ELEC-SAMSUNG-OLED55",
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
                    BrandId = appleBrandId,
                    Name = "Wireless Noise Cancelling Headphones",
                    Slug = "wireless-noise-cancelling-headphones",
                    Sku = "ELEC-APPLE-ANC40",
                    Description = "Active noise cancelling (ANC) headphones with up to 40 hours of battery life and studio-grade acoustics.",
                    Price = 14999.00m,
                    StockQuantity = 3,
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
                    BrandId = samsungBrandId,
                    Name = "Pro Edition Smartphone",
                    Slug = "pro-edition-smartphone",
                    Sku = "ELEC-SAMSUNG-PRO108",
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
                    BrandId = nikeBrandId,
                    Name = "Classic Black Leather Jacket",
                    Slug = "classic-black-leather-jacket",
                    Sku = "CLOTH-NIKE-LEATHER-BLK",
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
                    BrandId = addyBrandId,
                    Name = "Ultra-Light Running Sneakers",
                    Slug = "ultra-light-running-sneakers",
                    Sku = "CLOTH-ADDY-RUN-ULTRA",
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
                    BrandId = genericBrandId,
                    Name = "Espresso Coffee Machine",
                    Slug = "espresso-coffee-machine",
                    Sku = "HOME-GEN-ESPRESSO15",
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
                    BrandId = genericBrandId,
                    Name = "Smart Touch Air Fryer",
                    Slug = "smart-touch-air-fryer",
                    Sku = "HOME-GEN-AIRFRY85",
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
                    BrandId = genericBrandId,
                    Name = "Clean Architecture by Robert C. Martin",
                    Slug = "clean-architecture-by-robert-martin",
                    Sku = "BOOK-GEN-CLEANARCH",
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
                    BrandId = genericBrandId,
                    Name = "The Pragmatic Programmer",
                    Slug = "the-pragmatic-programmer",
                    Sku = "BOOK-GEN-PRAGMATIC",
                    Description = "One of the most significant books in software development, detailing best engineering habits and professional practices.",
                    Price = 999.00m,
                    StockQuantity = 2,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow,
                    Images =
                    [
                        new ProductImage { Url = "https://images.unsplash.com/photo-1629654297299-c8506221ca97?auto=format&fit=crop&w=600&q=80", IsPrimary = true }
                    ]
                }
            };

            foreach (var p in products)
            {
                p.IsPublished = true;
                if (p.Name is "Espresso Coffee Machine" or "Smart Touch Air Fryer" or "Pro Edition Smartphone")
                {
                    p.IsFeatured = true;
                }
                else if (p.Name is "Ultra-Light Running Sneakers" or "Smart OLED TV 55 inch")
                {
                    p.IsNewArrival = true;
                }
                else if (p.Name is "The Pragmatic Programmer" or "Clean Architecture by Robert C. Martin")
                {
                    p.IsBestSeller = true;
                }
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // 7. Seed Warehouse & Inventory
        Warehouse defaultWarehouse;
        if (!await context.Warehouses.AnyAsync())
        {
            defaultWarehouse = new Warehouse
            {
                Name = "Main Warehouse",
                Code = "WH-MAIN",
                Address = "123 E-Commerce Blvd, Tech City"
            };
            await context.Warehouses.AddAsync(defaultWarehouse);
            await context.SaveChangesAsync();
        }
        else
        {
            defaultWarehouse = await context.Warehouses.FirstAsync();
        }

        if (!await context.Inventories.AnyAsync())
        {
            var products = await context.Products.ToListAsync();
            foreach (var prod in products)
            {
                await context.Inventories.AddAsync(new Inventory
                {
                    ProductId = prod.Id,
                    Sku = prod.Sku,
                    WarehouseId = defaultWarehouse.Id,
                    CurrentStock = prod.StockQuantity,
                    AvailableStock = prod.StockQuantity,
                    ReservedStock = 0,
                    MinimumStockLevel = prod.MinimumStock,
                    MaximumStockLevel = prod.MaximumStock,
                    ReorderLevel = prod.MinimumStock + 5,
                    UnitCost = prod.CostPrice > 0 ? prod.CostPrice : prod.Price * 0.6m,
                    SellingPrice = prod.Price,
                    LastPurchasePrice = prod.CostPrice > 0 ? prod.CostPrice : prod.Price * 0.6m
                });
            }
            await context.SaveChangesAsync();
        }

        // 8. Seed AppSettings
        if (!await context.AppSettings.AnyAsync())
        {
            await context.AppSettings.AddRangeAsync(
                new AppSetting { Key = "MinOrderFreeShipping", Value = "500", Description = "Minimum order amount for free shipping" },
                new AppSetting { Key = "TaxRate", Value = "0.18", Description = "Default GST tax rate" }
            );
            await context.SaveChangesAsync();
        }

        // 9. Seed Couriers
        if (!await context.Couriers.AnyAsync())
        {
            await context.Couriers.AddRangeAsync(
                new Courier { Id = Guid.NewGuid(), Name = "DHL Express", Code = "DHL", Contact = "+1-800-225-5345", Website = "https://www.dhl.com", Status = "Active", EstimatedDeliveryTime = "1-3 Business Days" },
                new Courier { Id = Guid.NewGuid(), Name = "FedEx Corporation", Code = "FEDEX", Contact = "+1-800-463-3339", Website = "https://www.fedex.com", Status = "Active", EstimatedDeliveryTime = "2-5 Business Days" },
                new Courier { Id = Guid.NewGuid(), Name = "UPS Delivery", Code = "UPS", Contact = "+1-800-742-5877", Website = "https://www.ups.com", Status = "Active", EstimatedDeliveryTime = "2-5 Business Days" }
            );
            await context.SaveChangesAsync();
        }

        // 10. Seed Shipping Methods
        if (!await context.ShippingMethods.AnyAsync())
        {
            await context.ShippingMethods.AddRangeAsync(
                new ShippingMethod { Id = Guid.NewGuid(), Code = "Standard", Name = "Standard Shipping", Description = "Delivery in 5-7 business days", Cost = 10m, EstimatedDays = 7, IsActive = true },
                new ShippingMethod { Id = Guid.NewGuid(), Code = "Express", Name = "Express Shipping", Description = "Delivery in 1-2 business days", Cost = 25m, EstimatedDays = 2, IsActive = true }
            );
            await context.SaveChangesAsync();
        }
    }
}
