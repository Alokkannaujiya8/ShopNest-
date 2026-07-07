# ShopNest Installation Guide

Follow these steps to set up a local development environment for the ShopNest solution.

---

## 1. Prerequisites

Ensure you have the following installed on your machine:
- **.NET 8.0 SDK** or later
- **Node.js** v20.x or later (includes `npm`)
- **SQL Server 2022** / Express Edition
- **Redis Server** (for distributed caching)

---

## 2. Backend API Setup

1. **Clone & Open Project**:
   Open a terminal and navigate to:
   `d:\Asp.net core_Project\E-Commerce Management System\ShopNest.API`

2. **Configure Local Settings**:
   Open `ShopNest.API/appsettings.json` and adjust the SQL Server connection string under `DefaultConnection`.

3. **Run Database Migrations**:
   Generate the database tables and apply initial seeds:
   ```bash
   dotnet ef database update --project ShopNest.Infrastructure --startup-project ShopNest.API
   ```

4. **Start API**:
   ```bash
   dotnet run --project ShopNest.API
   ```
   The backend API will start at `http://localhost:5000`. Swagger documentation is accessible at `http://localhost:5000/swagger`.

---

## 3. Frontend Angular Setup

1. **Navigate to App Dir**:
   `cd d:\Asp.net core_Project\E-Commerce Management System\Frontend\shop-nest`

2. **Install Node Packages**:
   ```bash
   npm install
   ```

3. **Start Development Server**:
   ```bash
   npm start
   ```
   Open `http://localhost:4200` in your web browser.
