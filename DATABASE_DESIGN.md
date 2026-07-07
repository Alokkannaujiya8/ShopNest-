# ShopNest Database Design

ShopNest utilizes SQL Server 2022 as its primary relational store, managed via Entity Framework Core using a Code-First migration approach.

---

## 1. Core Tables Schema Directory

### Users & Authentication
- **AppUsers**: Holds user profiles (ID, Email, PasswordHash, FullName, Role, IsDeleted).
- **RefreshTokens**: Linked to users (Token, ExpiresUtc, RevokedAtUtc).
- **LoginHistories**: Tracks OS, IP, Browser, and session timestamps.

### Catalog Management
- **Categories**: Tree structures supporting parent-child self-referential links (`ParentCategoryId`).
- **Brands**: Product manufacturers metadata.
- **Products**: Main catalog info (Sku, Name, Slug, Price, IsActive, IsDeleted).
- **ProductImages**: Gallery media.
- **ProductVariants**: Specific size/color combinations.
- **InventoryStocks**: Quantity tracking by product/warehouse.

### Orders & Tracking
- **Orders**: Tracks buyers, totals, discount coupons, and payment states.
- **OrderItems**: Quantities and prices of items at order time.
- **Payments**: Linked to orders with Stripe transaction IDs.
- **OrderStatusHistories**: Milestones history.
- **OrderTrackings**: Shipment tracking details.

---

## 2. Global Query Filters & Indexes

- **Soft Delete filter**: EF Core automatically filters out soft-deleted items:
  ```csharp
  modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
  ```
- **Indexes**:
  - `IX_Products_Sku` (Unique index on product SKU).
  - `IX_Products_Slug` (Unique index for clean SEO URLs).
  - `IX_AppUsers_Email` (Unique index for lightning-fast logins).
  - `IX_AuditLogs_EntityId` (Index on audited entity logs).
