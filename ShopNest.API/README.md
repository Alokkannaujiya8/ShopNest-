# ShopNest API

ASP.NET Core 8 Web API backend for an e-commerce management system.

## Features

- JWT auth with refresh tokens and Admin/Customer roles
- Product CRUD, categories, search/filter/pagination, Cloudinary image upload
- Cart, checkout, order status tracking
- Stripe-ready payment sessions and webhook endpoint
- RabbitMQ order events
- Redis product query caching
- Elasticsearch product indexing hook with SQL search fallback
- SignalR order status hub at `/hubs/orders`
- Hangfire background job dashboard at `/hangfire`
- Swagger/OpenAPI at `/swagger`
- Docker Compose for SQL Server, Redis, RabbitMQ, Elasticsearch, and API

## Run

```powershell
dotnet restore
dotnet ef database update --project ShopNest.Infrastructure --startup-project ShopNest.API
dotnet run --project ShopNest.API
```

Or with Docker:

```powershell
docker compose up --build
```
