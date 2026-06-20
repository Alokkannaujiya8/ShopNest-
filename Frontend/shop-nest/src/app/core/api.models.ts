export type UserRole = 'Customer' | 'Admin';
export type OrderStatus = 'Pending' | 'Shipped' | 'Delivered' | 'Cancelled';
export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed' | 'Refunded';

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
  refreshToken: string;
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
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  category: Category;
  images: ProductImage[];
}

export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  total: number;
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
}

export interface Order {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  totalAmount: number;
  shippingAddress: string;
  items: OrderItem[];
  payment?: Payment | null;
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
