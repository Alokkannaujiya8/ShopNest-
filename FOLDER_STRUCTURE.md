# ShopNest Folder Structure

This document describes the directory tree layout for the ShopNest project.

---

## 1. Directory Tree Layout

```
E-Commerce Management System/
│
├── .github/
│   └── workflows/
│       └── ci-cd.yml                 # GitHub Actions CI/CD workflows
│
├── ShopNest.API/                     # Backend Solution Directory
│   ├── ShopNest.API/                 # Presentation / API Layer (Controllers, Middlewares, Program.cs)
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   └── appsettings.json
│   │
│   ├── ShopNest.Application/         # Business Logic Layer (CQRS Handlers, DTOs, Maps)
│   │   ├── Common/                   # Shared models (ApiResponse, PagedResult)
│   │   ├── Features/                 # MediatR commands, queries, and handlers
│   │   └── Interfaces/               # Application-wide interfaces (persistence contracts)
│   │
│   ├── ShopNest.Domain/              # Domain Core Layer (Domain Models, Value Objects, Enums)
│   │   ├── Entities/                 # Database tables entities
│   │   └── Enums/                    # Business enum flags
│   │
│   ├── ShopNest.Infrastructure/      # Persistence & External Services implementation
│   │   ├── Context/                  # ApplicationDbContext and DB Configurations
│   │   ├── Migrations/               # Entity Framework database schema migrations
│   │   └── Services/                 # Implementation of caching, search, reports, and emails
│   │
│   └── ShopNest.Tests/               # xUnit Testing Project
│
└── Frontend/                         # Frontend UI Directory
    └── shop-nest/                    # Angular 20 Source Directory
        ├── src/
        │   ├── app/                    # Angular components, services, and routing
        │   │   ├── core/               # Http intercepts, Auth/SignalR services, guards
        │   │   └── pages/              # Catalog, Cart, Profile, Admin Pages
        │   └── styles.scss             # SCSS theme systems
        └── package.json
```
