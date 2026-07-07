# ShopNest Project Overview

ShopNest is a premium, enterprise-grade E-Commerce Management System built to optimize shopping catalogs, order processing, and administrative controls.

---

## 1. Technical Stack Summary

- **Backend Framework**: ASP.NET Core 8 Web API
- **Persistence Store**: Microsoft SQL Server 2022 via EF Core
- **Caching Infrastructure**: Redis Distributed Cache
- **Task Orchestration**: Hangfire Background Jobs
- **Message Dispatching**: MediatR (CQRS Pattern)
- **Frontend Client**: Angular 20 (Responsive CSS grid, reactive forms, Guards, and Signals)
- **Testing Engine**: xUnit, FluentAssertions

---

## 2. Business Capabilities Directory

### 🛒 Customer-Facing Features
- Responsive product catalog searching, filtering, and sorting.
- Multi-currency product spec comparison grid.
- Dynamic cart operations, coupon application, and Stripe checkout pipelines.
- Order shipment status tracking timeline.

### 🛡️ Administrative Dashboard
- Complete CRUD controls for categories, products, inventory warehouses, and brands.
- System metrics tracking, audit change-logs, and error event viewers.
- CSV bulk product uploads.
- Monthly revenue sales report PDF generator.
