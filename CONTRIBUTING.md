# Contributing to ShopNest

We welcome contributions to ShopNest! To maintain the high architectural standards, performance, and security of this codebase, please follow the guidelines below.

---

## 1. Clean Architecture & SOLID Guidelines

- **Domain Isolation**: Never add dependencies to third-party database adapters, UI modules, or presentation packages inside `ShopNest.Domain`.
- **CQRS Pattern**: Business queries and commands must use MediatR. Keep controllers thin by routing all requests to their handlers.
- **Repository Interface Separation**: Declare repository interfaces inside `ShopNest.Application/Interfaces` and implement them inside `ShopNest.Infrastructure/Services` or `Persistence`.

---

## 2. Coding Standards

- **ASP.NET Core Best Practices**:
  - Keep all database, network, and disk actions asynchronous using `async`/`await`.
  - Always validate incoming requests using FluentValidation.
  - Implement dynamic route segments and enforce resource authorization using ASP.NET Core Policies.
- **Angular 20 Conventions**:
  - Prefer Standalone components unless utilizing the modular component framework.
  - Avoid state mutations outside of NgRx actions.
  - Always clean up RxJS subscriptions using `takeUntil` patterns or Signals.

---

## 3. Pull Request Guidelines

1. **Create Feature Branch**: Use prefixing (e.g. `feat/auth-validation`, `fix/cache-eviction`).
2. **Compile Check**: Run `dotnet build` to verify the codebase compiles with zero errors.
3. **Verify Tests**: Run `dotnet test` to confirm all test suites pass.
4. **Clean Commits**: Format commit titles cleanly (e.g. `feat: implement OTP template system`).
