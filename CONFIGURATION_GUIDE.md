# ShopNest Configuration Guide

This guide walks through configuring the ShopNest API and frontend clients.

---

## 1. Application Settings (`appsettings.json`)

The primary configuration resides in `ShopNest.API/ShopNest.API/appsettings.json`.

### Database Connection Strings
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=ShopNestDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### JWT Security Token Configurations
```json
"JwtSettings": {
  "Secret": "YOUR_SECRET_KEY_MUST_BE_AT_LEAST_32_CHARACTERS_LONG",
  "Issuer": "ShopNestAPI",
  "Audience": "ShopNestClients",
  "ExpiryInMinutes": 60,
  "RefreshExpiryInDays": 7
}
```

### SMTP Email Settings
```json
"SmtpSettings": {
  "Server": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-specific-password",
  "SenderEmail": "noreply@shopnest.com",
  "SenderName": "ShopNest System"
}
```

### Stripe Payment Keys
```json
"Stripe": {
  "PublishableKey": "pk_test_...",
  "SecretKey": "sk_test_...",
  "WebhookSecret": "whsec_..."
}
```

---

## 2. Distributed Caching & Event Broker Settings

- **Redis Connection**: Set `RedisSettings:ConnectionString` to your server (e.g. `localhost:6379`).
- **RabbitMQ Details**: Enter host credentials in `RabbitMQ:Host` (e.g. `localhost`).
