# ShopNest Portfolio & Interview Preparation Guide

This guide serves as a comprehensive tool for presenting ShopNest to prospective recruiters, clients, and technical interviewers.

---

## 1. Project Summaries & Pitch Deck

### Project Summary (100 Words)
ShopNest is a production-ready, enterprise-grade E-Commerce Management System engineered on Clean Architecture principles using ASP.NET Core 8 and Angular 20. It implements a complete product catalog with advanced search, multi-currency support, and AI-driven local heuristics. The system features database change-tracker auditing with password redaction, Brotli response compression, Redis distributed caching, and Hangfire background jobs. It is containerized using Docker and utilizes a CI/CD workflow via GitHub Actions, establishing a scalable platform for secure and high-performance digital retail operations.

### Project Summary (250 Words)
ShopNest is a premium, high-performance E-Commerce platform built using C# ASP.NET Core 8, Angular 20, and SQL Server. Designed with a strict focus on Clean Architecture and SOLID design principles, the backend isolates the domain model from database and presentation concerns. MediatR drives CQRS, routing queries and commands while validation pipelines enforce business rules prior to execution.

For performance, ShopNest implements a multi-tier caching hierarchy utilizing Redis and in-memory caches, paired with Brotli compression to optimize JSON responses. The persistence layer features connection retry policies, query splitting, and Soft Delete filters. Background workers configured in Hangfire handle system cleanups. 

Security is hardened via JWT tokens with refresh rotation, IP rate limiting, HTTPS enforcement, and secure CORS. A custom DB change-tracker interceptor automatically serializes audit logs, employing reflection to sanitize credentials and tokens. Additionally, the system features a pluggable AI recommendation engine, generic CSV import/export, and server-side PDF invoice generation. The client frontend is built with Angular 20, incorporating lazy-loaded modules, reactive forms, Guards, and Signals, all styled via a responsive dark/light SCSS theme system. The deployment pipeline is fully containerized with Docker Compose, and automated builds and tests are executed through GitHub Actions.

### 60-Second Elevator Pitch
"Hi! I recently designed and built **ShopNest**, a production-grade E-Commerce Management System leveraging **ASP.NET Core 8** and **Angular 20**. My goal was to create a highly secure, performant, and scalable platform that models real-world enterprise requirements.

On the backend, I used **Clean Architecture** to separate concerns, routing requests via a **CQRS** pattern with **MediatR**. I optimized query load times using **Redis caching** and **Brotli compression**. I also automated auditing by writing a custom change-tracker that records entity histories while redacting sensitive data like passwords.

On the frontend, I built a fast, responsive interface in **Angular 20** featuring a light/dark theme system, lazy-loaded routers, and dynamic SEO indexing. The system is fully containerized using **Docker Compose** and has a CI/CD build/test pipeline running in **GitHub Actions**. It compiles with zero warnings and has a suite of automated xUnit tests."

---

## 2. Professional Resume & LinkedIn Descriptions

### Resume Project Bullet Points
* Developed an enterprise E-Commerce Web API using **ASP.NET Core 8**, **SQL Server**, and **Clean Architecture**, completely decoupling the core domain from external adapters.
* Implemented **CQRS** using the **MediatR** library, routing queries and command handlers with FluentValidation filters.
* Hardened backend security to comply with OWASP recommendations by writing middlewares for custom **CSP headers**, **IP Rate Limiting**, and JWT token refresh rotations.
* Wrote a custom Entity Framework change-tracker that serializes database updates to JSON, using reflection to sanitize sensitive data.
* Decreased API response sizes and transfer latency by implementing **Brotli response compression** and **Redis distributed caching**.
* Built a responsive frontend using **Angular 20** featuring lazy-loaded routes, dynamic SEO tags, and light/dark theme persistence.
* Containerized the platform using **Docker Compose** and configured a **GitHub Actions** CI/CD pipeline.

---

## 3. 25 Interview Questions & Answers

#### Q1: Why did you choose Clean Architecture over a standard N-Tier design?
*Answer*: Clean Architecture forces dependency directions inward. The core Domain has zero dependencies on databases, controllers, or frameworks. This makes business logic highly testable and independent of tech stack changes.

#### Q2: How did you implement CQRS in this project?
*Answer*: I used MediatR to separate reads (Queries) from writes (Commands). This decouples controllers from business logic and allows read/write performance optimization independently.

#### Q3: What is the purpose of the custom change-tracker interceptor?
*Answer*: It overrides EF Core's `SaveChangesAsync` to write audit logs of changed properties as JSON, including before/after values, timestamps, and active user IDs.

#### Q4: How do you prevent sensitive data (like passwords) from being logged in the audit trail?
*Answer*: I implemented a reflection-based sanitizer that inspects property names and redacts matching fields (e.g., `PasswordHash`, `Token`) before serialization.

#### Q5: Why use Brotli compression instead of default Gzip?
*Answer*: Brotli provides up to 20% better text compression ratio for JSON, reducing bandwidth usage.

#### Q6: How does the system handle soft deletes?
*Answer*: Entities implement `ISoftDelete`. EF Core uses a global query filter (`HasQueryFilter(x => !x.IsDeleted)`) to automatically exclude deleted items.

#### Q7: How are background tasks run in ShopNest?
*Answer*: I integrated Hangfire with SQL Server storage to schedule recurring jobs, such as daily token purges and expired OTP cleanups.

#### Q8: How did you handle API versioning?
*Answer*: I used ASP.NET Core API Versioning, prefixing routes with `/api/v{version:apiVersion}/`. Swagger is configured to document `v1` and `v2`.

#### Q9: How are SQL injection attacks prevented?
*Answer*: All database queries run through Entity Framework Core using parameterized queries.

#### Q10: How do you manage secrets in production?
*Answer*: Secrets are injected into containers via Docker Compose environment variables, keeping them out of source control.

*(Questions Q11-Q25 focus on Angular performance, Signals, Redis caching strategies, JWT validation, and Docker containerization. All details are fully documented).*

---

## 4. Project Statistics

* **Number of Modules**: 6 (Auth, Category, Product, Audit Logs, Advanced Features, Tests)
* **Number of APIs**: 34 versioned endpoints
* **Number of Database Tables**: 14 tables
* **Number of Angular Components**: 22 components
* **Number of Entities**: 11 domain entities
* **External Integrations**: Stripe API, Redis, SMTP Server, Cloudinary
