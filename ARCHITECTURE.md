# ShopNest Architecture Documentation

ShopNest is built adhering strictly to Clean Architecture boundaries, SOLID development patterns, and asynchronous workflows.

---

## 1. Clean Architecture Architectural Layers

```
      +-------------------------------------------------------------+
      |                       Presentation                          |
      |   (ShopNest.API Controllers / Angular 20 Frontend Views)    |
      +-----------------------------+-------------------------------+
                                    |
                                    v
      +-------------------------------------------------------------+
      |                       Infrastructure                        |
      |          (Persistence, Redis Cache, Stripe API, PDF)        |
      +-----------------------------+-------------------------------+
                                    |
                                    v
      +-------------------------------------------------------------+
      |                        Application                          |
      |               (CQRS Commands/Queries, MediatR)              |
      +-----------------------------+-------------------------------+
                                    |
                                    v
      +-------------------------------------------------------------+
      |                          Domain                             |
      |                  (Core Entities, Enums)                     |
      +-------------------------------------------------------------+
```

### Layer Responsibilities
- **Domain**: Self-contained Core layer containing entities and enterprise business rules. Contains zero third-party package dependencies.
- **Application**: The coordinating brain of the application. Contains interface contracts, FluentValidation models, DTOs, and MediatR handlers.
- **Infrastructure**: Implements persistence protocols (SQL Server via Entity Framework Core), caching engines (Redis), background scheduling (Hangfire), and helper utilities.
- **Presentation**: Version-segmented controllers handling HTTP requests and serializing responses.

---

## 2. CQRS Flow & MediatR Pipeline

Every client request hits the presentation layer, which forwards the request to the application layer via MediatR pipeline commands:

```
[Client Request] 
      │
      ▼
[Controller Route]
      │
      ▼ (Send Query/Command)
[MediatR Mediator]
      │
      ├─► [Validation Behavior] (FluentValidation checks schema)
      │
      ▼ (Valid request)
[MediatR Handler] ────► [Repository/Db] ────► [JSON Response]
```

This guarantees thin controllers and clear separation of read/write concerns.
