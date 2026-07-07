# ShopNest API Documentation

All API endpoints are version-segmented under `/api/v1/`.

---

## 1. Authentication APIs

### Register User
`POST /api/v1/auth/register`
- **Body**: `{ "email": "...", "password": "...", "fullName": "..." }`
- **Response**: `200 OK` (User registered successfully)

### Login User
`POST /api/v1/auth/login`
- **Body**: `{ "email": "...", "password": "..." }`
- **Response**: JWT Token, Refresh Token, User metadata.

---

## 2. Product Catalog APIs

### Get Products List
`GET /api/v1/products`
- **Query Params**: `page`, `pageSize`, `search`, `categoryId`, `minPrice`, `maxPrice`
- **Response**: Paged list of products.

### Create Product
`POST /api/v1/products`
- **Headers**: `Authorization: Bearer <token>`
- **Authorized Roles**: Admin, SuperAdmin
- **Body**: Product details object.

---

## 3. Order Management APIs

### Place Order
`POST /api/v1/orders`
- **Headers**: `Authorization: Bearer <token>`
- **Body**: `{ "items": [...], "shippingAddress": "...", "couponCode": "..." }`

---

## 4. Advanced Services APIs

### AI Recommendations
`GET /api/v1/advanced/recommendations/user/{id}`
- Returns tailored products list based on purchase logs.

### CSV Import
`POST /api/v1/advanced/import/products/csv`
- Imports bulk product rows.
