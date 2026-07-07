export type UserRole = 'Customer' | 'Admin' | 'Seller';
export type OrderStatus = 'Pending' | 'Shipped' | 'Delivered' | 'Cancelled';
export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed' | 'Refunded';

export interface ApiResponse<T = any> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AuthResponse {
  userId: string;
  fullName: string;
  email: string;
  role: UserRole;
  accessToken: string;
  refreshToken?: string | null;
  accessTokenExpiresAtUtc: string;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
}

export interface ProductImage {
  id: string;
  url: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductAttributeValue {
  id: string;
  productAttributeId: string;
  attributeName: string;
  value: string;
}

export interface ProductVariant {
  id: string;
  name: string;
  sku: string;
  barcode?: string | null;
  price: number;
  stockQuantity: number;
  imageUrl?: string | null;
  isActive: boolean;
  attributeValues: ProductAttributeValue[];
}

export interface Product {
  id: string;
  name: string;
  sku: string;
  barcode?: string | null;
  slug: string;
  shortDescription?: string | null;
  description: string;
  categoryId: string;
  category?: Category | null;
  subCategoryId?: string | null;
  isDeleted: boolean;
  subCategory?: Category | null;
  brandId?: string | null;
  brandName?: string | null;
  costPrice: number;
  price: number;
  discountType?: string | null;
  discountValue: number;
  discountStartDate?: string | null;
  discountEndDate?: string | null;
  taxPercentage: number;
  stockQuantity: number;
  minimumStock: number;
  maximumStock: number;
  stockStatus: string;
  weight: number;
  length: number;
  width: number;
  height: number;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
  isFeatured: boolean;
  isNewArrival: boolean;
  isBestSeller: boolean;
  isActive: boolean;
  isPublished: boolean;
  averageRating: number;
  reviewsCount: number;
  images: ProductImage[];
  variants: ProductVariant[];
}

export interface ProductAttribute {
  id: string;
  name: string;
}

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  brandName?: string | null;
  categoryName: string;
  imageUrl?: string | null;
  unitPrice: number;
  originalPrice: number;
  discountPrice: number;
  quantity: number;
  lineTotal: number;
  stockStatus: string;
  availableQuantity: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subtotal: number;
  totalDiscount: number;
  appliedCouponCode?: string | null;
  couponDiscount: number;
  shippingCharges: number;
  estimatedTax: number;
  grandTotal: number;
}

export interface Payment {
  id: string;
  provider: string;
  providerPaymentId: string;
  providerOrderId: string;
  status: PaymentStatus;
  amount: number;
  currency: string;
}

export interface OrderItem {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  productVariantId?: string | null;
  sku?: string;
  discount?: number;
  tax?: number;
  total?: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  totalAmount: number;
  shippingAddress: string;
  billingAddress?: string | null;
  paymentMethod?: string | null;
  courierPartner?: string | null;
  trackingNumber?: string | null;
  shippingCost?: number;
  tax?: number;
  discount?: number;
  orderNotes?: string | null;
  estimatedDeliveryDate?: string | null;
  deliveredDate?: string | null;
  items: OrderItem[];
  payment?: Payment | null;
}

export interface OrderStatusHistory {
  id: string;
  status: OrderStatus;
  note: string;
  changedBy: string;
  createdAtUtc: string;
}

export interface OrderTracking {
  id: string;
  courierPartner: string;
  trackingNumber: string;
  status: string;
  location: string;
  createdAtUtc: string;
}

export interface DashboardStats {
  customers: number;
  products: number;
  lowStockProducts: number;
  orders: number;
  revenue: number;
  pendingOrders: number;
}

export interface PaymentSessionResponse {
  paymentId: string;
  provider: string;
  clientSecret: string;
  providerOrderId: string;
}

export interface UserProfileDto {
  userId: string;
  fullName: string;
  email: string;
  mobileNumber: string;
  dateOfBirth?: string | null;
  gender?: string | null;
  bio?: string | null;
  profilePictureUrl?: string | null;
}

export interface UpdateProfileRequest {
  fullName: string;
  mobileNumber: string;
  dateOfBirth?: string | null;
  gender?: string | null;
  bio?: string | null;
}

export interface UserAddressDto {
  id: string;
  fullName: string;
  mobileNumber: string;
  alternateMobile?: string | null;
  country: string;
  state: string;
  city: string;
  area: string;
  landmark?: string | null;
  postalCode: string;
  addressLine1: string;
  addressLine2?: string | null;
  addressType: string; // 'Home' | 'Office' | 'Other'
  email?: string | null;
  deliveryInstructions?: string | null;
  isDefault: boolean;
}

export interface AddAddressRequest {
  fullName: string;
  mobileNumber: string;
  alternateMobile?: string | null;
  country: string;
  state: string;
  city: string;
  area: string;
  landmark?: string | null;
  postalCode: string;
  addressLine1: string;
  addressLine2?: string | null;
  addressType: string;
  email?: string | null;
  deliveryInstructions?: string | null;
  isDefault: boolean;
}

export interface ChangeProfilePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface AdminUserDto {
  id: string;
  fullName: string;
  email: string;
  mobileNumber: string;
  role: string;
  isActive: boolean;
  isLocked: boolean;
  lockoutEndUtc?: string | null;
  lastLoginUtc?: string | null;
  loginCount: number;
  emailVerified: boolean;
  mobileVerified: boolean;
  createdAtUtc: string;
}

export interface AdminRoleDto {
  id: string;
  name: string;
  displayName: string;
  description?: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface CreateAdminUserRequest {
  fullName: string;
  email: string;
  mobileNumber: string;
  password: string;
  role: string;
}

export interface UpdateAdminUserRequest {
  fullName: string;
  mobileNumber: string;
  role: string;
  isActive: boolean;
}

export interface CreateRoleRequest {
  name: string;
  displayName: string;
  description?: string | null;
}

export interface UpdateRoleRequest {
  displayName: string;
  description?: string | null;
  isActive: boolean;
}

export interface AdminResetPasswordRequest {
  newPassword: string;
  confirmPassword: string;
  forcePasswordChange: boolean;
}

export interface AdminCategoryDto {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  shortDescription?: string | null;
  imageUrl?: string | null;
  bannerUrl?: string | null;
  parentId?: string | null;
  parentName?: string | null;
  displayOrder: number;
  isFeatured: boolean;
  isActive: boolean;
  isDeleted: boolean;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
  createdAtUtc: string;
  childrenCount: number;
}

export interface CategoryNodeDto {
  id: string;
  name: string;
  slug: string;
  imageUrl?: string | null;
  displayOrder: number;
  isActive: boolean;
  children: CategoryNodeDto[];
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
  shortDescription?: string | null;
  parentId?: string | null;
  displayOrder: number;
  isFeatured: boolean;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string | null;
  shortDescription?: string | null;
  parentId?: string | null;
  displayOrder: number;
  isFeatured: boolean;
  isActive: boolean;
  metaTitle?: string | null;
  metaDescription?: string | null;
  metaKeywords?: string | null;
}

export interface Warehouse {
  id: string;
  name: string;
  code: string;
  address?: string | null;
}

export interface Inventory {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  productVariantId?: string | null;
  productVariantName?: string | null;
  sku: string;
  warehouseId?: string | null;
  warehouseName?: string | null;
  currentStock: number;
  availableStock: number;
  reservedStock: number;
  minimumStockLevel: number;
  maximumStockLevel: number;
  reorderLevel: number;
  unitCost: number;
  sellingPrice: number;
  lastPurchasePrice: number;
  lastUpdated: string;
}

export interface InventoryTransaction {
  id: string;
  transactionNumber: string;
  inventoryId: string;
  productName: string;
  variantName?: string | null;
  sku: string;
  quantity: number;
  previousStock: number;
  updatedStock: number;
  transactionType: string;
  reason?: string | null;
  performedBy: string;
  referenceNumber?: string | null;
  transactionDate: string;
}

export interface StockInRequest {
  productId: string;
  productVariantId?: string | null;
  warehouseId?: string | null;
  quantity: number;
  unitCost: number;
  performedBy?: string | null;
  reason?: string | null;
  referenceNumber?: string | null;
}

export interface StockOutRequest {
  productId: string;
  productVariantId?: string | null;
  warehouseId?: string | null;
  quantity: number;
  performedBy?: string | null;
  reason?: string | null;
  referenceNumber?: string | null;
}

export interface StockAdjustmentRequest {
  productId: string;
  productVariantId?: string | null;
  warehouseId?: string | null;
  newQuantity: number;
  performedBy?: string | null;
  reason: string;
  referenceNumber?: string | null;
}

export interface UpdateInventoryLimitsRequest {
  productId: string;
  productVariantId?: string | null;
  warehouseId?: string | null;
  minimumStockLevel: number;
  maximumStockLevel: number;
  reorderLevel: number;
}

export interface WishlistItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  productSlug: string;
  brandName?: string | null;
  categoryName: string;
  price: number;
  originalPrice: number;
  discountValue: number;
  stockQuantity: number;
  stockStatus: string;
  averageRating: number;
  reviewsCount: number;
  imageUrl?: string | null;
  createdAtUtc: string;
}

export interface WishlistSearchRequest {
  query?: string | null;
  categoryId?: string | null;
  brandId?: string | null;
  stockStatus?: string | null;
  isDiscounted?: boolean | null;
  sortBy?: string | null;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ShippingMethod {
  code: string;
  name: string;
  description: string;
  cost: number;
  estimatedDays: number;
}

export interface CheckoutSummary {
  cart: Cart;
  addresses: UserAddressDto[];
  shippingMethods: ShippingMethod[];
  paymentMethods: string[];
}

export interface CheckoutValidationRequest {
  shippingAddressId: string;
  shippingMethodCode: string;
  paymentMethod: string;
}

export interface CheckoutValidationResult {
  isValid: boolean;
  errorMessage?: string | null;
  subtotal: number;
  discount: number;
  shippingCost: number;
  tax: number;
  grandTotal: number;
}

export interface PlaceOrderRequest {
  shippingAddressId: string;
  shippingMethodCode: string;
  paymentMethod: string;
  orderNotes?: string | null;
  deliveryInstructions?: string | null;
}

export interface Courier {
  id: string;
  name: string;
  code: string;
  contact: string;
  website: string;
  status: string;
  estimatedDeliveryTime: string;
}

export interface ShipmentTracking {
  id: string;
  shipmentId: string;
  status: string;
  location: string;
  description: string;
  createdAtUtc: string;
}

export interface Shipment {
  id: string;
  shipmentNumber: string;
  trackingNumber: string;
  orderId: string;
  orderNumber: string;
  courier?: Courier | null;
  shippingAddress: string;
  billingAddress: string;
  shipmentDate?: string | null;
  pickupDate?: string | null;
  estimatedDeliveryDate?: string | null;
  deliveredDate?: string | null;
  shippingCharges: number;
  deliveryInstructions: string;
  status: string;
  notes: string;
  trackingHistory: ShipmentTracking[];
}

export interface Review {
  id: string;
  productId: string;
  productName: string;
  userId: string;
  userFullName: string;
  orderId?: string | null;
  rating: number;
  reviewTitle: string;
  reviewDescription: string;
  isRecommended: boolean;
  helpfulCount: number;
  status: string;
  adminNotes: string;
  isReported: boolean;
  reportReason: string;
  reviewImages: string[];
  createdAtUtc: string;
  updatedAtUtc?: string | null;
  hasLiked: boolean;
}

export interface RatingSummary {
  averageRating: number;
  totalReviews: number;
  ratingDistribution: { [key: number]: number };
}

export interface ProductAnswer {
  id: string;
  questionId: string;
  userId: string;
  userFullName: string;
  answerText: string;
  isAdminOrSeller: boolean;
  createdAtUtc: string;
}

export interface ProductQuestion {
  id: string;
  productId: string;
  productName: string;
  userId: string;
  userFullName: string;
  questionText: string;
  createdAtUtc: string;
  answers: ProductAnswer[];
}

export interface Notification {
  id: string;
  userId: string;
  userFullName: string;
  title: string;
  message: string;
  notificationType: string; // Information, Success, Warning, Error, Promotion
  channel: string; // Email, InApp, SMS, Push, WhatsApp, Telegram
  priority: string; // Low, Medium, High
  status: string; // Pending, Sent, Failed
  relatedEntity: string;
  relatedEntityId: string;
  isRead: boolean;
  sentTime?: string | null;
}

export interface NotificationTemplate {
  id: string;
  code: string;
  name: string;
  subject: string;
  body: string;
  channel: string;
}

export interface NotificationLog {
  id: string;
  notificationId: string;
  channel: string;
  recipient: string;
  status: string;
  errorMessage: string;
  sentAtUtc: string;
}

export interface DashboardSummary {
  totalUsers: number;
  totalCustomers: number;
  totalAdmins: number;
  totalProducts: number;
  activeProducts: number;
  outOfStockProducts: number;
  lowStockProducts: number;
  totalCategories: number;
  totalBrands: number;
  totalOrders: number;
  pendingOrders: number;
  processingOrders: number;
  deliveredOrders: number;
  cancelledOrders: number;
  returnedOrders: number;
  totalRevenue: number;
  monthlyRevenue: number;
  todayRevenue: number;
  averageOrderValue: number;
}

export interface SalesInterval {
  interval: string;
  totalSales: number;
  orderCount: number;
}

export interface CategorySales {
  categoryName: string;
  revenue: number;
  itemsSold: number;
}

export interface BrandSales {
  brandName: string;
  revenue: number;
  itemsSold: number;
}

export interface ProductSales {
  productName: string;
  sku: string;
  revenue: number;
  quantitySold: number;
}

export interface RevenueTrend {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface SalesReport {
  dailySales: SalesInterval[];
  weeklySales: SalesInterval[];
  monthlySales: SalesInterval[];
  yearlySales: SalesInterval[];
  salesByCategory: CategorySales[];
  salesByBrand: BrandSales[];
  salesByProduct: ProductSales[];
  topSellingProducts: ProductSales[];
  topSellingCategories: CategorySales[];
  revenueTrends: RevenueTrend[];
}

export interface RevenueReport {
  grossRevenue: number;
  discountGiven: number;
  netRevenue: number;
  totalTax: number;
  totalShippingCost: number;
  revenueTrends: RevenueTrend[];
}

export interface OrderReportItem {
  orderId: string;
  orderNumber: string;
  customerName: string;
  status: string;
  totalAmount: number;
  createdAtUtc: string;
  paymentMethod?: string | null;
}

export interface OrdersReport {
  totalOrders: number;
  pendingCount: number;
  deliveredCount: number;
  cancelledCount: number;
  returnedCount: number;
  totalRefunded: number;
  orders: OrderReportItem[];
}

export interface CustomerReportItem {
  userId: string;
  fullName: string;
  email: string;
  orderCount: number;
  totalSpent: number;
}

export interface CustomerPurchaseHistory {
  userId: string;
  fullName: string;
  orderNumber: string;
  amount: number;
  orderDate: string;
  status: string;
}

export interface CustomerReport {
  totalCustomers: number;
  newCustomersCount: number;
  activeCustomersCount: number;
  topCustomers: CustomerReportItem[];
  purchaseHistories: CustomerPurchaseHistory[];
}

export interface ProductPerformance {
  productId: string;
  productName: string;
  sku: string;
  quantitySold: number;
  revenueGenerated: number;
}

export interface ProductStock {
  productId: string;
  productName: string;
  sku: string;
  stockQuantity: number;
  price: number;
}

export interface ProductRating {
  productId: string;
  productName: string;
  averageRating: number;
  reviewCount: number;
}

export interface ProductReport {
  performance: ProductPerformance[];
  lowStockProducts: ProductStock[];
  outOfStockProducts: ProductStock[];
  bestRated: ProductRating[];
  worstRated: ProductRating[];
}

export interface InventoryTransactionSummary {
  transactionId: string;
  productName: string;
  sku: string;
  transactionType: string;
  quantity: number;
  reason: string;
  createdAtUtc: string;
}

export interface InventoryReport {
  outOfStockCount: number;
  lowStockCount: number;
  inStockCount: number;
  totalStockQuantity: number;
  recentTransactions: InventoryTransactionSummary[];
}

export interface PaymentMethodUsage {
  method: string;
  usageCount: number;
  totalAmount: number;
}

export interface PaymentReportItem {
  paymentId: string;
  orderNumber: string;
  customerName: string;
  amount: number;
  provider: string;
  status: string;
  createdAtUtc: string;
}

export interface PaymentReport {
  successCount: number;
  failedCount: number;
  pendingCount: number;
  totalRefunded: number;
  methodUsage: PaymentMethodUsage[];
  recentPayments: PaymentReportItem[];
}

export interface CouponUsageSummary {
  code: string;
  usageCount: number;
  totalDiscountGiven: number;
}

export interface CouponPerformance {
  code: string;
  minOrderAmount: number;
  discountValue: number;
  isPercent: boolean;
  usageCount: number;
  totalSavings: number;
}

export interface CouponReport {
  usage: CouponUsageSummary[];
  performance: CouponPerformance[];
  totalDiscountGiven: number;
}

export interface MostReviewedProduct {
  productId: string;
  productName: string;
  reviewCount: number;
  averageRating: number;
}

export interface ReviewReport {
  averageRating: number;
  ratingDistribution: { [key: number]: number };
  mostReviewed: MostReviewedProduct[];
}
