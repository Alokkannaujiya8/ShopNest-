# ShopNest - Enterprise E-Commerce Management System

ShopNest is a production-ready, enterprise-grade E-Commerce Management System built on Clean Architecture principles, leveraging **ASP.NET Core 8** and **Angular 20**.

---

## 📚 Technical Documentation Index

To explore the architecture, database models, configurations, and deployment strategies of this system, refer to the following documents:

* 📄 **[Project Overview](PROJECT_OVERVIEW.md)**: Product goals, full feature matrix, and technical stack summary.
* 📐 **[System Architecture](ARCHITECTURE.md)**: Clean Architecture boundaries, CQRS patterns, and MediatR command/query handlers flow.
* 📁 **[Folder Structure Map](FOLDER_STRUCTURE.md)**: Tree view layout of the API layers and Angular directory paths.
* 🔌 **[API Route Catalog](API_DOCUMENTATION.md)**: Endpoints, authentication policies, input DTO models, and Swagger settings.
* 🗄️ **[Database Design](DATABASE_DESIGN.md)**: SQL Server tables, keys, indexing strategies, and soft-delete query filters.
* ⚙️ **[Configuration Guide](CONFIGURATION_GUIDE.md)**: Environment variables schema, JWT parameters, SMTP, and Stripe keys.
* 💻 **[Installation Guide](INSTALLATION_GUIDE.md)**: Step-by-step instructions to run the API and Angular frontend locally.
* 🚀 **[Deployment Guide](DEPLOYMENT_GUIDE.md)**: Virtual Host configurations for IIS, Linux/Nginx reverse proxy, and Docker Compose profiles.
* 👥 **[Contributing Rules](CONTRIBUTING.md)**: Development guidelines, coding conventions, and pull request steps.
* 📝 **[Changelog](CHANGELOG.md)**: Release histories and implemented milestone additions.
* 💼 **[Portfolio & Interview Prep Guide](PORTFOLIO_GUIDE.md)**: Elevator pitch, resume descriptions, and 25 technical Q&A interview cards.

---

## 🚀 Quick Start (Local Run)

### 1. Backend API Setup
Configure your SQL Server connection in `ShopNest.API/ShopNest.API/appsettings.json` and execute:
```bash
cd ShopNest.API
dotnet ef database update --project ShopNest.Infrastructure --startup-project ShopNest.API
dotnet run --project ShopNest.API
```
The API Swagger documentation will be available at `http://localhost:5000/swagger`.

### 2. Frontend Angular Setup
Navigate to the Angular frontend directory and run:
```bash
cd Frontend/shop-nest
npm install
npm start
```
The client dashboard will load at `http://localhost:4200`.

### 3. Docker Compose (DevOps Stack)
Build and spin up the complete orchestration stack (API, SQL Server, Redis, and RabbitMQ):
```bash
cd ShopNest.API
docker-compose up --build -d
```
Access points:
- Swagger UI: `http://localhost:5000/swagger`
- Hangfire Panel: `http://localhost:5000/hangfire`
- Health Probe: `http://localhost:5000/health`
