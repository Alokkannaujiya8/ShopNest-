-- Optimal indexes for reporting query acceleration
CREATE NONCLUSTERED INDEX IX_Orders_Reporting 
ON [dbo].[Orders] ([Status], [CreatedAtUtc]) 
INCLUDE ([TotalAmount], [Discount], [Tax], [ShippingCost], [UserId]);

CREATE NONCLUSTERED INDEX IX_OrderItems_Reporting
ON [dbo].[OrderItems] ([ProductId])
INCLUDE ([OrderId], [Quantity], [Total]);

CREATE NONCLUSTERED INDEX IX_Products_Reporting
ON [dbo].[Products] ([IsActive], [IsPublished])
INCLUDE ([CategoryId], [BrandId], [StockQuantity], [MinimumStock], [Price]);

CREATE NONCLUSTERED INDEX IX_Reviews_Reporting
ON [dbo].[Reviews] ([Status], [Rating])
INCLUDE ([ProductId]);

CREATE NONCLUSTERED INDEX IX_Payments_Reporting
ON [dbo].[Payments] ([Status], [CreatedAtUtc])
INCLUDE ([Amount], [Provider]);

CREATE NONCLUSTERED INDEX IX_Refunds_Reporting
ON [dbo].[Refunds] ([Status], [CreatedAtUtc])
INCLUDE ([Amount]);

CREATE NONCLUSTERED INDEX IX_CouponUsages_Reporting
ON [dbo].[CouponUsages] ([CreatedAtUtc])
INCLUDE ([CouponId], [DiscountAmount]);

CREATE NONCLUSTERED INDEX IX_AuditLogs_Reporting
ON [dbo].[AuditLogs] ([Action], [CreatedAtUtc])
INCLUDE ([UserId]);
