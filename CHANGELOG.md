# Changelog

All notable changes to the ShopNest project will be documented in this file.

---

## [1.0.0] - 2026-07-05
### Added
- **Core Security**: BCrypt password encryption, JWT refresh token rotations, correlation ID logs, and IP rate limits.
- **Audit Trails**: Global Change-Tracker audit logging with reflection redaction of passwords/payment variables.
- **AI Recommendation Heuristics**: Co-occurrence analytics and user purchase vectors for item matches.
- **Bulk Import/Export Engine**: Reflection-based CSV parser supporting bulk category/product roundtrip updates.
- **Enterprise PDF Engine**: Server-side receipt invoice templates and monthly sales analytics reporting.
- **Multi-Currency System**: Price converters for INR, USD, and EUR.
- **Background Handlers**: Hangfire daily purges of expired session records.
- **CI/CD Configuration**: GitHub Actions yaml pipeline compiling, testing, and packaging container tags.

### Fixed
- Fixed Brand soft-delete references.
- Corrected database timeline mapping variables in Order status history.
