# ShopNest Enterprise Frontend Application

ShopNest is an enterprise-grade e-commerce application powered by **Angular 20** and integrated with a high-performance **ASP.NET Core Web API** backend.

---

## 🛠️ Project Architecture

The architecture is built following modular Angular practices:
- **Core Module (`src/app/core/`)**: Holds singleton services, state management, interceptors, routing guards, and models.
- **Shared Module (`src/app/shared/`)**: Holds reusable components (Loader, Toast, Breadcrumbs, etc.).
- **Pages Module (`src/app/pages/`)**: Contains modular components for individual application screens (Catalog, Cart, Wishlist, Profile, Notifications, Admin panels).

---

## ⚙️ Environment Configuration

The application interfaces with the backend services through configurations located in:
- Base API Endpoint: `https://localhost:7002/api`
- Real-time SignalR Hubs: `https://localhost:7002/hubs/orders`

---

## 🚀 Getting Started

### 1. Prerequisites
- **Node.js**: v20.x or higher
- **Angular CLI**: v20.x

### 2. Installation
Install project dependencies:
```bash
npm install
```

### 3. Running Development Server
Start the local server:
```bash
npm run start
```
Navigate to `http://localhost:4200/` in your browser.

### 4. Production Compilation
Compile optimized production assets:
```bash
npm run build
```

---

## 🧪 Testing Verification
- **Unit Testing**: Run `npm run test` (Karma + Jasmine runner) to verify application logic.
- **E2E Testing**: Configure Playwright or run `ng e2e` tests to validate customer checkout and admin dashboard flows.
