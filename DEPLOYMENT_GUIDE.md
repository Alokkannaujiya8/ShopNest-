# ShopNest Deployment Guide

This guide details configuring and deploying the ShopNest system in enterprise production environments.

---

## 1. IIS Deployment (Windows Server)

1. **Publish API Project**:
   ```bash
   dotnet publish ShopNest.API.csproj -c Release -o C:\inetpub\shopnest-api
   ```
2. **Configure IIS Site**:
   - Install **.NET Core Hosting Bundle** on the server.
   - Create a new Website pointing to the output directory.
   - Set the Application Pool to **No Managed Code**.
3. **Environment Configuration**:
   Create a web.config or set System Environment Variables for connection strings and JWT signing secrets.

---

## 2. Docker & Compose Orchestration

To run the production-ready compiled container grid:
```bash
docker-compose -f docker-compose.yml up --build -d
```
This starts:
- **ShopNest.API Container** (port 5000)
- **SQL Server 2022** (port 1433)
- **Redis Cache Grid** (port 6379)
- **RabbitMQ Broker** (port 5672)

---

## 3. Linux + Nginx Deployment

Deploying the ASP.NET Core service behind an Nginx reverse proxy:

### Nginx Virtual Host Config
```nginx
server {
    listen 80;
    server_name api.shopnest.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```
Use `systemd` to keep the dotnet app running as a background service.
