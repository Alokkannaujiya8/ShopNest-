import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  AuthResponse,
  Cart,
  Category,
  DashboardStats,
  Order,
  OrderStatus,
  PagedResult,
  PaymentSessionResponse,
  Product,
} from './api.models';

export const API_BASE_URL = 'https://localhost:7002/api';

@Injectable({ providedIn: 'root' })
export class ApiService {
  readonly currentUser = signal<AuthResponse | null>(this.readSession());
  readonly cartItemCount = signal<number>(0);

  constructor(private readonly http: HttpClient) {
    if (this.currentUser()) {
      this.refreshCartCount();
    }
  }

  refreshCartCount(): void {
    this.cart().subscribe({
      next: (c) => this.updateCartItemCount(c),
      error: () => this.cartItemCount.set(0)
    });
  }

  private updateCartItemCount(cart: Cart): void {
    const count = cart.items ? cart.items.reduce((sum, item) => sum + item.quantity, 0) : 0;
    this.cartItemCount.set(count);
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/auth/login`, { email, password })
      .pipe(
        tap((session) => {
          this.saveSession(session);
          this.refreshCartCount();
        })
      );
  }

  register(fullName: string, email: string, password: string, role: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE_URL}/auth/register`, { fullName, email, password, role })
      .pipe(
        tap((session) => {
          this.saveSession(session);
          this.refreshCartCount();
        })
      );
  }

  logout(): void {
    localStorage.removeItem('shopnest.session');
    this.currentUser.set(null);
    this.cartItemCount.set(0);
  }

  products(filters: Record<string, string | number | boolean | null | undefined>): Observable<PagedResult<Product>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') params = params.set(key, String(value));
    });
    return this.http.get<PagedResult<Product>>(`${API_BASE_URL}/products`, { params });
  }

  product(id: string): Observable<Product> {
    return this.http.get<Product>(`${API_BASE_URL}/products/${id}`);
  }

  createProduct(payload: Partial<Product> & { categoryId: string }): Observable<Product> {
    return this.http.post<Product>(`${API_BASE_URL}/products`, payload);
  }

  updateProduct(id: string, payload: Partial<Product> & { categoryId: string }): Observable<Product> {
    return this.http.put<Product>(`${API_BASE_URL}/products/${id}`, payload);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE_URL}/products/${id}`);
  }

  uploadProductImage(productId: string, file: File, isPrimary: boolean): Observable<unknown> {
    const form = new FormData();
    form.append('file', file);
    form.append('isPrimary', String(isPrimary));
    return this.http.post(`${API_BASE_URL}/products/${productId}/images`, form);
  }

  categories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${API_BASE_URL}/categories`);
  }

  createCategory(name: string): Observable<Category> {
    return this.http.post<Category>(`${API_BASE_URL}/categories`, { name });
  }

  cart(): Observable<Cart> {
    return this.http.get<Cart>(`${API_BASE_URL}/cart`).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  addToCart(productId: string, quantity: number): Observable<Cart> {
    return this.http.post<Cart>(`${API_BASE_URL}/cart/items`, { productId, quantity }).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  updateCartItem(itemId: string, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`${API_BASE_URL}/cart/items/${itemId}`, { quantity }).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  removeCartItem(itemId: string): Observable<Cart> {
    return this.http.delete<Cart>(`${API_BASE_URL}/cart/items/${itemId}`).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  checkout(shippingAddress: string, paymentProvider = 'stripe'): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/cart/checkout`, { shippingAddress, paymentProvider }).pipe(
      tap(() => this.cartItemCount.set(0))
    );
  }

  myOrders(page = 1): Observable<PagedResult<Order>> {
    return this.http.get<PagedResult<Order>>(`${API_BASE_URL}/orders/my`, { params: { page, pageSize: 20 } });
  }

  allOrders(page = 1): Observable<PagedResult<Order>> {
    return this.http.get<PagedResult<Order>>(`${API_BASE_URL}/admin/orders`, { params: { page, pageSize: 20 } });
  }

  updateOrderStatus(orderId: string, status: OrderStatus): Observable<Order> {
    return this.http.patch<Order>(`${API_BASE_URL}/orders/${orderId}/status`, { status });
  }

  createPayment(orderId: string, provider = 'stripe'): Observable<PaymentSessionResponse> {
    return this.http.post<PaymentSessionResponse>(`${API_BASE_URL}/payments`, { orderId, provider });
  }

  completePayment(paymentId: string, status: string): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/payments/complete`, { paymentId, status });
  }

  dashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${API_BASE_URL}/admin/dashboard`);
  }

  updateInventory(productId: string, stockQuantity: number, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${API_BASE_URL}/admin/inventory/${productId}`, { stockQuantity, isActive });
  }

  private saveSession(session: AuthResponse): void {
    localStorage.setItem('shopnest.session', JSON.stringify(session));
    this.currentUser.set(session);
  }

  private readSession(): AuthResponse | null {
    const raw = localStorage.getItem('shopnest.session');
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}
