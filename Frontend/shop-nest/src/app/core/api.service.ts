import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap, map } from 'rxjs';
import {
  ApiResponse,
  AuthResponse,
  Cart,
  Category,
  DashboardStats,
  Order,
  OrderStatus,
  PagedResult,
  PaymentSessionResponse,
  Product,
  ProductAttribute,
  ProductVariant,
  ProductImage,
  UserProfileDto,
  UpdateProfileRequest,
  UserAddressDto,
  AddAddressRequest,
  ChangeProfilePasswordRequest,
  AdminUserDto,
  AdminRoleDto,
  CreateAdminUserRequest,
  UpdateAdminUserRequest,
  CreateRoleRequest,
  UpdateRoleRequest,
  AdminResetPasswordRequest,
  AdminCategoryDto,
  CategoryNodeDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  Warehouse,
  Inventory,
  InventoryTransaction,
  StockInRequest,
  StockOutRequest,
  StockAdjustmentRequest,
  UpdateInventoryLimitsRequest,
  WishlistItem,
  WishlistSearchRequest,
  ShippingMethod,
  CheckoutSummary,
  CheckoutValidationRequest,
  CheckoutValidationResult,
  PlaceOrderRequest,
  OrderStatusHistory,
  OrderTracking,
  Courier,
  Shipment,
  ShipmentTracking,
  Review,
  RatingSummary,
  ProductQuestion,
  ProductAnswer,
  Notification,
  NotificationTemplate,
  NotificationLog,
  DashboardSummary,
  SalesReport,
  RevenueReport,
  OrdersReport,
  CustomerReport,
  ProductReport,
  InventoryReport,
  PaymentReport,
  CouponReport,
  ReviewReport,
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

  login(email: string, password: string, rememberMe = false): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${API_BASE_URL}/auth/login`, { email, password, rememberMe })
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            this.saveSession(res.data);
            this.refreshCartCount();
          }
        })
      );
  }

  register(payload: {
    fullName: string;
    email: string;
    mobileNumber: string;
    password: string;
    confirmPassword: string;
    acceptTerms: boolean;
    role: string;
  }): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/auth/register`, payload);
  }

  verifyEmailOtp(email: string, otp: string): Observable<ApiResponse<AuthResponse>> {
    return this.http
      .post<ApiResponse<AuthResponse>>(`${API_BASE_URL}/auth/verify-email-otp`, { email, otp })
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            this.saveSession(res.data);
            this.refreshCartCount();
          }
        })
      );
  }

  resendEmailOtp(email: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/auth/resend-email-otp`, { email });
  }

  forgotPassword(email: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/auth/forgot-password`, { email });
  }

  resetPassword(email: string, otp: string, newPassword: string, confirmPassword: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/auth/reset-password`, { email, otp, newPassword, confirmPassword });
  }

  logout(): void {
    this.http.post(`${API_BASE_URL}/auth/logout`, {}).subscribe({
      next: () => this.clearSession(),
      error: () => this.clearSession()
    });
  }

  private clearSession(): void {
    localStorage.removeItem('shopnest.session');
    this.currentUser.set(null);
    this.cartItemCount.set(0);
  }

  products(filters: Record<string, string | number | boolean | null | undefined>): Observable<PagedResult<Product>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') params = params.set(key, String(value));
    });
    return this.http.get<ApiResponse<PagedResult<Product>>>(`${API_BASE_URL}/products`, { params }).pipe(
      map(res => res.data || { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 })
    );
  }

  product(id: string): Observable<Product> {
    return this.http.get<ApiResponse<Product>>(`${API_BASE_URL}/products/${id}`).pipe(
      map(res => {
        if (!res.data) throw new Error(res.message || 'Product not found.');
        return res.data;
      })
    );
  }

  createProduct(payload: Partial<Product> & { categoryId: string }): Observable<Product> {
    return this.http.post<ApiResponse<Product>>(`${API_BASE_URL}/products`, payload).pipe(
      map(res => {
        if (!res.data) throw new Error(res.message || 'Failed to create product.');
        return res.data;
      })
    );
  }

  updateProduct(id: string, payload: Partial<Product> & { categoryId: string }): Observable<Product> {
    return this.http.put<ApiResponse<Product>>(`${API_BASE_URL}/products/${id}`, payload).pipe(
      map(res => {
        if (!res.data) throw new Error(res.message || 'Failed to update product.');
        return res.data;
      })
    );
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/products/${id}`).pipe(
      map(() => undefined)
    );
  }

  uploadProductImage(productId: string, file: File, isPrimary: boolean): Observable<unknown> {
    const form = new FormData();
    form.append('file', file);
    form.append('isPrimary', String(isPrimary));
    return this.http.post(`${API_BASE_URL}/products/${productId}/images`, form);
  }

  categories(): Observable<Category[]> {
    return this.http.get<ApiResponse<PagedResult<Category>>>(`${API_BASE_URL}/categories`).pipe(
      map(res => res.data?.items || [])
    );
  }

  createCategory(name: string): Observable<Category> {
    return this.http.post<ApiResponse<Category>>(`${API_BASE_URL}/categories`, { name }).pipe(
      map(res => {
        if (!res.data) throw new Error(res.message || 'Failed to create category.');
        return res.data;
      })
    );
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

  increaseCartItemQuantity(itemId: string): Observable<Cart> {
    return this.http.put<Cart>(`${API_BASE_URL}/cart/items/${itemId}/increase`, {}).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  decreaseCartItemQuantity(itemId: string): Observable<Cart> {
    return this.http.put<Cart>(`${API_BASE_URL}/cart/items/${itemId}/decrease`, {}).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  removeCartItem(itemId: string): Observable<Cart> {
    return this.http.delete<Cart>(`${API_BASE_URL}/cart/items/${itemId}`).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  clearCart(): Observable<Cart> {
    return this.http.delete<Cart>(`${API_BASE_URL}/cart/clear`).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  applyCoupon(couponCode: string): Observable<Cart> {
    return this.http.post<Cart>(`${API_BASE_URL}/cart/coupon/apply`, JSON.stringify(couponCode), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  removeCoupon(): Observable<Cart> {
    return this.http.delete<Cart>(`${API_BASE_URL}/cart/coupon/remove`).pipe(
      tap(cart => this.updateCartItemCount(cart))
    );
  }

  moveWishlistItemToCart(productId: string): Observable<Cart> {
    return this.http.post<Cart>(`${API_BASE_URL}/cart/items/move-from-wishlist/${productId}`, {}).pipe(
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

  getOrderByNumber(orderNumber: string): Observable<Order> {
    return this.http.get<Order>(`${API_BASE_URL}/orders/number/${orderNumber}`);
  }

  getOrderById(orderId: string, dummy: null = null): Observable<Order> {
    return this.http.get<Order>(`${API_BASE_URL}/orders/${orderId}`);
  }

  cancelOrder(orderId: string, reason: string): Observable<Order> {
    return this.http.patch<Order>(`${API_BASE_URL}/orders/${orderId}/cancel`, { reason });
  }

  updatePaymentStatus(orderId: string, paymentStatus: string): Observable<Order> {
    return this.http.patch<Order>(`${API_BASE_URL}/orders/${orderId}/payment-status`, { paymentStatus });
  }

  requestReturn(orderId: string, reason: string): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/orders/${orderId}/return`, { reason });
  }

  requestRefund(orderId: string, reason: string): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/orders/${orderId}/refund`, { reason });
  }

  assignCourier(orderId: string, courierPartner: string, trackingNumber: string): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/orders/${orderId}/courier`, { courierPartner, trackingNumber });
  }

  getOrderTimeline(orderId: string): Observable<OrderStatusHistory[]> {
    return this.http.get<OrderStatusHistory[]>(`${API_BASE_URL}/orders/${orderId}/timeline`);
  }

  getOrderTracking(orderId: string): Observable<OrderTracking[]> {
    return this.http.get<OrderTracking[]>(`${API_BASE_URL}/orders/${orderId}/track`);
  }

  addTrackingUpdate(orderId: string, status: string, location: string): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/orders/${orderId}/tracking-update`, { status, location });
  }

  getInvoiceDownloadUrl(orderId: string): string {
    return `${API_BASE_URL}/orders/${orderId}/invoice`;
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

  profile(): Observable<ApiResponse<UserProfileDto>> {
    return this.http.get<ApiResponse<UserProfileDto>>(`${API_BASE_URL}/profile`);
  }

  updateProfile(payload: UpdateProfileRequest): Observable<ApiResponse<UserProfileDto>> {
    return this.http.put<ApiResponse<UserProfileDto>>(`${API_BASE_URL}/profile`, payload).pipe(
      tap(res => {
        if (res.success && res.data) {
          const current = this.currentUser();
          if (current) {
            this.saveSession({
              ...current,
              fullName: res.data.fullName
            } as any);
          }
        }
      })
    );
  }

  uploadProfilePicture(file: File): Observable<ApiResponse<UserProfileDto>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<UserProfileDto>>(`${API_BASE_URL}/profile/picture`, form);
  }

  deleteProfilePicture(): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/profile/picture`);
  }

  addresses(): Observable<ApiResponse<UserAddressDto[]>> {
    return this.http.get<ApiResponse<UserAddressDto[]>>(`${API_BASE_URL}/profile/addresses`);
  }

  addAddress(payload: AddAddressRequest): Observable<ApiResponse<UserAddressDto>> {
    return this.http.post<ApiResponse<UserAddressDto>>(`${API_BASE_URL}/profile/addresses`, payload);
  }

  updateAddress(id: string, payload: AddAddressRequest): Observable<ApiResponse<UserAddressDto>> {
    return this.http.put<ApiResponse<UserAddressDto>>(`${API_BASE_URL}/profile/addresses/${id}`, payload);
  }

  deleteAddress(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/profile/addresses/${id}`);
  }

  setDefaultAddress(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/profile/addresses/${id}/default`, {});
  }

  changePassword(payload: ChangeProfilePasswordRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/profile/password`, payload);
  }

  profileOrders(): Observable<ApiResponse<Order[]>> {
    return this.http.get<ApiResponse<Order[]>>(`${API_BASE_URL}/profile/orders`);
  }

  // ==========================================
  // ADMIN USER & ROLE MANAGEMENT
  // ==========================================
  getAdminUsers(query: {
    search?: string;
    role?: string;
    sortBy?: string;
    descending?: boolean;
    page: number;
    pageSize: number;
  }): Observable<ApiResponse<PagedResult<AdminUserDto>>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) params = params.set('search', query.search);
    if (query.role) params = params.set('role', query.role);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.descending !== undefined) params = params.set('descending', query.descending.toString());

    return this.http.get<ApiResponse<PagedResult<AdminUserDto>>>(`${API_BASE_URL}/admin/management/users`, { params });
  }

  getAdminUserById(id: string): Observable<ApiResponse<AdminUserDto>> {
    return this.http.get<ApiResponse<AdminUserDto>>(`${API_BASE_URL}/admin/management/users/${id}`);
  }

  createAdminUser(request: CreateAdminUserRequest): Observable<ApiResponse<AdminUserDto>> {
    return this.http.post<ApiResponse<AdminUserDto>>(`${API_BASE_URL}/admin/management/users`, request);
  }

  updateAdminUser(id: string, request: UpdateAdminUserRequest): Observable<ApiResponse<AdminUserDto>> {
    return this.http.put<ApiResponse<AdminUserDto>>(`${API_BASE_URL}/admin/management/users/${id}`, request);
  }

  deleteAdminUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}`);
  }

  activateAdminUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}/activate`, {});
  }

  deactivateAdminUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}/deactivate`, {});
  }

  lockAdminUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}/lock`, {});
  }

  unlockAdminUser(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}/unlock`, {});
  }

  resetAdminUserPassword(id: string, request: AdminResetPasswordRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${id}/reset-password`, request);
  }

  getAdminRoles(query: {
    search?: string;
    page: number;
    pageSize: number;
  }): Observable<ApiResponse<PagedResult<AdminRoleDto>>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) params = params.set('search', query.search);

    return this.http.get<ApiResponse<PagedResult<AdminRoleDto>>>(`${API_BASE_URL}/admin/management/roles`, { params });
  }

  getAdminRoleById(id: string): Observable<ApiResponse<AdminRoleDto>> {
    return this.http.get<ApiResponse<AdminRoleDto>>(`${API_BASE_URL}/admin/management/roles/${id}`);
  }

  createAdminRole(request: CreateRoleRequest): Observable<ApiResponse<AdminRoleDto>> {
    return this.http.post<ApiResponse<AdminRoleDto>>(`${API_BASE_URL}/admin/management/roles`, request);
  }

  updateAdminRole(id: string, request: UpdateRoleRequest): Observable<ApiResponse<AdminRoleDto>> {
    return this.http.put<ApiResponse<AdminRoleDto>>(`${API_BASE_URL}/admin/management/roles/${id}`, request);
  }

  deleteAdminRole(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/roles/${id}`);
  }

  activateAdminRole(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/roles/${id}/activate`, {});
  }

  deactivateAdminRole(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/roles/${id}/deactivate`, {});
  }

  restoreAdminRole(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/roles/${id}/restore`, {});
  }

  assignAdminRole(userId: string, roleId: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${userId}/assign-role/${roleId}`, {});
  }

  removeAdminRole(userId: string, roleId: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/admin/management/users/${userId}/remove-role/${roleId}`, {});
  }

  // ==========================================
  // CATEGORY MANAGEMENT
  // ==========================================
  getAdminCategories(query: {
    search?: string;
    isActive?: boolean;
    isFeatured?: boolean;
    parentId?: string;
    isDeleted?: boolean;
    sortBy?: string;
    descending?: boolean;
    page: number;
    pageSize: number;
  }): Observable<ApiResponse<PagedResult<AdminCategoryDto>>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) params = params.set('search', query.search);
    if (query.isActive !== undefined) params = params.set('isActive', query.isActive.toString());
    if (query.isFeatured !== undefined) params = params.set('isFeatured', query.isFeatured.toString());
    if (query.parentId) params = params.set('parentId', query.parentId);
    if (query.isDeleted !== undefined) params = params.set('isDeleted', query.isDeleted.toString());
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.descending !== undefined) params = params.set('descending', query.descending.toString());

    return this.http.get<ApiResponse<PagedResult<AdminCategoryDto>>>(`${API_BASE_URL}/categories`, { params });
  }

  getCategoryById(id: string): Observable<ApiResponse<AdminCategoryDto>> {
    return this.http.get<ApiResponse<AdminCategoryDto>>(`${API_BASE_URL}/categories/${id}`);
  }

  getCategoryTree(): Observable<ApiResponse<CategoryNodeDto[]>> {
    return this.http.get<ApiResponse<CategoryNodeDto[]>>(`${API_BASE_URL}/categories/tree`);
  }

  getParentCategoriesList(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${API_BASE_URL}/categories/parents`);
  }

  adminCreateCategory(request: CreateCategoryRequest): Observable<ApiResponse<AdminCategoryDto>> {
    return this.http.post<ApiResponse<AdminCategoryDto>>(`${API_BASE_URL}/categories`, request);
  }

  adminUpdateCategory(id: string, request: UpdateCategoryRequest): Observable<ApiResponse<AdminCategoryDto>> {
    return this.http.put<ApiResponse<AdminCategoryDto>>(`${API_BASE_URL}/categories/${id}`, request);
  }

  deleteCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/categories/${id}`);
  }

  activateCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/categories/${id}/activate`, {});
  }

  deactivateCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/categories/${id}/deactivate`, {});
  }

  restoreCategory(id: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${API_BASE_URL}/categories/${id}/restore`, {});
  }

  uploadCategoryImage(id: string, file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<string>>(`${API_BASE_URL}/categories/${id}/image`, formData);
  }

  uploadCategoryBanner(id: string, file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<string>>(`${API_BASE_URL}/categories/${id}/banner`, formData);
  }

  searchProducts(filters: any): Observable<PagedResult<Product>> {
    return this.products(filters);
  }

  getProductById(id: string): Observable<Product> {
    return this.product(id);
  }

  getProductBySlug(slug: string): Observable<Product> {
    return this.http.get<ApiResponse<Product>>(`${API_BASE_URL}/products/slug/${slug}`).pipe(
      map(res => {
        if (!res.data) throw new Error(res.message || 'Product not found.');
        return res.data;
      })
    );
  }

  getFeaturedProducts(count = 10): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/products/featured?count=${count}`).pipe(
      map(res => res.data || [])
    );
  }

  getBestSellerProducts(count = 10): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/products/bestseller?count=${count}`).pipe(
      map(res => res.data || [])
    );
  }

  getNewArrivalProducts(count = 10): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/products/new-arrival?count=${count}`).pipe(
      map(res => res.data || [])
    );
  }

  getProductAttributes(): Observable<ProductAttribute[]> {
    return this.http.get<ProductAttribute[]>(`${API_BASE_URL}/products/attributes`);
  }

  createProductAttribute(name: string): Observable<ProductAttribute> {
    return this.http.post<ProductAttribute>(`${API_BASE_URL}/products/attributes`, { name });
  }

  restoreProduct(id: string): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/restore`, {});
  }

  activateProduct(id: string): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/activate`, {});
  }

  deactivateProduct(id: string): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/deactivate`, {});
  }

  publishProduct(id: string): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/publish`, {});
  }

  unpublishProduct(id: string): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/unpublish`, {});
  }

  duplicateProduct(id: string): Observable<Product> {
    return this.http.post<Product>(`${API_BASE_URL}/products/${id}/duplicate`, {});
  }

  deleteProductImage(id: string, imageId: string): Observable<any> {
    return this.http.delete(`${API_BASE_URL}/products/${id}/images/${imageId}`);
  }

  reorderProductImages(id: string, requests: { imageId: string; displayOrder: number }[]): Observable<any> {
    return this.http.put(`${API_BASE_URL}/products/${id}/images/reorder`, requests);
  }

  searchInventory(filters: any): Observable<PagedResult<Inventory>> {
    let params = new HttpParams();
    if (filters) {
      Object.keys(filters).forEach(key => {
        if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
          params = params.set(key, filters[key].toString());
        }
      });
    }
    return this.http.get<PagedResult<Inventory>>(`${API_BASE_URL}/inventory`, { params });
  }

  getInventoryByProduct(productId: string, variantId?: string, warehouseId?: string): Observable<Inventory> {
    let url = `${API_BASE_URL}/inventory/product/${productId}`;
    let params = new HttpParams();
    if (variantId) params = params.set('productVariantId', variantId);
    if (warehouseId) params = params.set('warehouseId', warehouseId);
    return this.http.get<Inventory>(url, { params });
  }

  getInventoryTransactions(filters: any): Observable<PagedResult<InventoryTransaction>> {
    let params = new HttpParams();
    if (filters) {
      Object.keys(filters).forEach(key => {
        if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
          params = params.set(key, filters[key].toString());
        }
      });
    }
    return this.http.get<PagedResult<InventoryTransaction>>(`${API_BASE_URL}/inventory/transactions`, { params });
  }

  getUserOrders(page: number, pageSize: number): Observable<PagedResult<Order>> {
    return this.http.get<PagedResult<Order>>(`${API_BASE_URL}/profile/orders?page=${page}&pageSize=${pageSize}`);
  }

  wishlist(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/profile/wishlist`);
  }

  addToWishlist(productId: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/profile/wishlist/${productId}`, {});
  }

  removeFromWishlist(productId: string): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${API_BASE_URL}/profile/wishlist/${productId}`);
  }

  moveWishlistToCart(productId: string): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${API_BASE_URL}/profile/wishlist/${productId}/move-to-cart`, {}).pipe(
      tap(res => {
        if (res.success) {
          this.refreshCartCount();
        }
      })
    );
  }

  getLowStockProducts(page: number, pageSize: number): Observable<PagedResult<Inventory>> {
    return this.http.get<PagedResult<Inventory>>(`${API_BASE_URL}/inventory/low-stock?page=${page}&pageSize=${pageSize}`);
  }

  getOutOfStockProducts(page: number, pageSize: number): Observable<PagedResult<Inventory>> {
    return this.http.get<PagedResult<Inventory>>(`${API_BASE_URL}/inventory/out-of-stock?page=${page}&pageSize=${pageSize}`);
  }

  stockIn(request: StockInRequest): Observable<Inventory> {
    return this.http.post<Inventory>(`${API_BASE_URL}/inventory/stock-in`, request);
  }

  stockOut(request: StockOutRequest): Observable<Inventory> {
    return this.http.post<Inventory>(`${API_BASE_URL}/inventory/stock-out`, request);
  }

  adjustStock(request: StockAdjustmentRequest): Observable<Inventory> {
    return this.http.post<Inventory>(`${API_BASE_URL}/inventory/adjust`, request);
  }

  updateInventoryLimits(request: UpdateInventoryLimitsRequest): Observable<Inventory> {
    return this.http.put<Inventory>(`${API_BASE_URL}/inventory/limits`, request);
  }

  getWarehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(`${API_BASE_URL}/inventory/warehouses`);
  }

  getSuggestions(query: string): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/products/suggestions?query=${query}`).pipe(
      map(res => res.data || [])
    );
  }

  getRelatedProducts(productId: string, count: number = 6): Observable<Product[]> {
    return this.http.get<ApiResponse<Product[]>>(`${API_BASE_URL}/products/${productId}/related?count=${count}`).pipe(
      map(res => res.data || [])
    );
  }

  getSearchHistory(): Observable<string[]> {
    return this.http.get<string[]>(`${API_BASE_URL}/products/search-history`);
  }

  getPopularSearches(count: number = 8): Observable<string[]> {
    return this.http.get<string[]>(`${API_BASE_URL}/products/popular-searches?count=${count}`);
  }

  getWishlistItems(filters: WishlistSearchRequest): Observable<PagedResult<WishlistItem>> {
    let params = new HttpParams();
    if (filters) {
      Object.keys(filters).forEach(key => {
        const val = (filters as any)[key];
        if (val !== null && val !== undefined && val !== '') {
          params = params.set(key, val.toString());
        }
      });
    }
    return this.http.get<PagedResult<WishlistItem>>(`${API_BASE_URL}/wishlist`, { params });
  }

  getWishlistCount(): Observable<number> {
    return this.http.get<number>(`${API_BASE_URL}/wishlist/count`);
  }

  addWishlistItem(productId: string): Observable<WishlistItem> {
    return this.http.post<WishlistItem>(`${API_BASE_URL}/wishlist/add/${productId}`, {});
  }

  removeWishlistItem(productId: string): Observable<boolean> {
    return this.http.delete<boolean>(`${API_BASE_URL}/wishlist/remove/${productId}`);
  }

  clearWishlistItems(): Observable<boolean> {
    return this.http.delete<boolean>(`${API_BASE_URL}/wishlist/clear`);
  }

  getCheckoutSummary(): Observable<CheckoutSummary> {
    return this.http.get<CheckoutSummary>(`${API_BASE_URL}/checkout/summary`);
  }

  getShippingMethods(): Observable<ShippingMethod[]> {
    return this.http.get<ShippingMethod[]>(`${API_BASE_URL}/checkout/shipping-methods`);
  }

  getPaymentMethods(): Observable<string[]> {
    return this.http.get<string[]>(`${API_BASE_URL}/checkout/payment-methods`);
  }

  validateCheckout(request: CheckoutValidationRequest): Observable<CheckoutValidationResult> {
    return this.http.post<CheckoutValidationResult>(`${API_BASE_URL}/checkout/validate`, request);
  }

  placeOrder(request: PlaceOrderRequest): Observable<Order> {
    return this.http.post<Order>(`${API_BASE_URL}/checkout/place-order`, request).pipe(
      tap(() => this.cartItemCount.set(0))
    );
  }

  initializePayment(request: { orderId: string, provider: string, currency: string }): Observable<any> {
    return this.http.post<any>(`${API_BASE_URL}/payment/initialize`, request);
  }

  verifyPayment(request: { paymentId: string, transactionId: string }): Observable<boolean> {
    return this.http.post<boolean>(`${API_BASE_URL}/payment/verify`, request);
  }

  getShipments(courier?: string, status?: string, page = 1): Observable<PagedResult<Shipment>> {
    const params: any = { page, pageSize: 20 };
    if (courier) params.courier = courier;
    if (status) params.status = status;
    return this.http.get<PagedResult<Shipment>>(`${API_BASE_URL}/shipping`, { params });
  }

  getShipmentById(id: string): Observable<Shipment> {
    return this.http.get<Shipment>(`${API_BASE_URL}/shipping/${id}`);
  }

  getShipmentByTrackingNumber(trackingNumber: string): Observable<Shipment> {
    return this.http.get<Shipment>(`${API_BASE_URL}/shipping/tracking/${trackingNumber}`);
  }

  getShipmentTimeline(id: string): Observable<ShipmentTracking[]> {
    return this.http.get<ShipmentTracking[]>(`${API_BASE_URL}/shipping/${id}/timeline`);
  }

  createShipment(request: { orderId: string, courierId?: string | null, manualTrackingNumber?: string | null, shippingAddress: string, billingAddress: string, shippingCharges: number, deliveryInstructions?: string | null }): Observable<Shipment> {
    return this.http.post<Shipment>(`${API_BASE_URL}/shipping`, request);
  }

  assignShipmentCourier(id: string, request: { courierId: string, manualTrackingNumber?: string | null }): Observable<Shipment> {
    return this.http.put<Shipment>(`${API_BASE_URL}/shipping/${id}/assign`, request);
  }

  updateShipmentStatus(id: string, request: { status: string, location: string, description?: string | null }): Observable<Shipment> {
    return this.http.put<Shipment>(`${API_BASE_URL}/shipping/${id}/status`, request);
  }

  rescheduleDelivery(id: string, request: { newEstimatedDeliveryDate: string, reason: string }): Observable<Shipment> {
    return this.http.put<Shipment>(`${API_BASE_URL}/shipping/${id}/reschedule`, request);
  }

  markShipmentDelivered(id: string): Observable<Shipment> {
    return this.http.put<Shipment>(`${API_BASE_URL}/shipping/${id}/deliver`, {});
  }

  markShipmentFailed(id: string, reason: string): Observable<Shipment> {
    return this.http.put<Shipment>(`${API_BASE_URL}/shipping/${id}/fail`, { reason });
  }

  getCouriers(): Observable<Courier[]> {
    return this.http.get<Courier[]>(`${API_BASE_URL}/shipping/couriers`);
  }

  createCourier(request: { name: string, code: string, contact: string, website: string, estimatedDeliveryTime: string }): Observable<Courier> {
    return this.http.post<Courier>(`${API_BASE_URL}/shipping/couriers`, request);
  }

  getReviews(productId?: string, userId?: string, status?: string, page = 1): Observable<PagedResult<Review>> {
    const params: any = { page, pageSize: 20 };
    if (productId) params.productId = productId;
    if (userId) params.userId = userId;
    if (status) params.status = status;
    return this.http.get<PagedResult<Review>>(`${API_BASE_URL}/reviews`, { params });
  }

  getReviewById(id: string): Observable<Review> {
    return this.http.get<Review>(`${API_BASE_URL}/reviews/${id}`);
  }

  getProductReviews(productId: string, page = 1): Observable<PagedResult<Review>> {
    const params = { page, pageSize: 10 };
    return this.http.get<PagedResult<Review>>(`${API_BASE_URL}/reviews/product/${productId}`, { params });
  }

  getRatingSummary(productId: string): Observable<RatingSummary> {
    return this.http.get<RatingSummary>(`${API_BASE_URL}/reviews/product/${productId}/summary`);
  }

  addReview(request: { productId: string, orderId?: string | null, rating: number, reviewTitle: string, reviewDescription: string, isRecommended: boolean, reviewImages?: string[] }): Observable<Review> {
    return this.http.post<Review>(`${API_BASE_URL}/reviews`, request);
  }

  updateReview(id: string, request: { rating: number, reviewTitle: string, reviewDescription: string, isRecommended: boolean, reviewImages?: string[] }): Observable<Review> {
    return this.http.put<Review>(`${API_BASE_URL}/reviews/${id}`, request);
  }

  deleteReview(id: string): Observable<any> {
    return this.http.delete<any>(`${API_BASE_URL}/reviews/${id}`);
  }

  restoreReview(id: string): Observable<any> {
    return this.http.post<any>(`${API_BASE_URL}/reviews/${id}/restore`, {});
  }

  reportReview(id: string, reason: string): Observable<any> {
    return this.http.post<any>(`${API_BASE_URL}/reviews/${id}/report`, { reason });
  }

  likeHelpfulReview(id: string): Observable<any> {
    return this.http.post<any>(`${API_BASE_URL}/reviews/${id}/helpful`, {});
  }

  unlikeHelpfulReview(id: string): Observable<any> {
    return this.http.delete<any>(`${API_BASE_URL}/reviews/${id}/helpful`);
  }

  moderateReview(id: string, status: string, adminNotes: string): Observable<Review> {
    return this.http.put<Review>(`${API_BASE_URL}/reviews/${id}/moderate`, { status, adminNotes });
  }

  getProductQuestions(productId: string, page = 1): Observable<PagedResult<ProductQuestion>> {
    const params = { page, pageSize: 15 };
    return this.http.get<PagedResult<ProductQuestion>>(`${API_BASE_URL}/reviews/product/${productId}/questions`, { params });
  }

  askQuestion(request: { productId: string, questionText: string }): Observable<ProductQuestion> {
    return this.http.post<ProductQuestion>(`${API_BASE_URL}/reviews/questions`, request);
  }

  replyToQuestion(questionId: string, answerText: string): Observable<ProductQuestion> {
    return this.http.post<ProductQuestion>(`${API_BASE_URL}/reviews/questions/${questionId}/reply`, { answerText });
  }

  updateQuestion(questionId: string, questionText: string): Observable<ProductQuestion> {
    return this.http.put<ProductQuestion>(`${API_BASE_URL}/reviews/questions/${questionId}`, { questionText });
  }

  updateAnswer(answerId: string, answerText: string): Observable<ProductAnswer> {
    return this.http.put<ProductAnswer>(`${API_BASE_URL}/reviews/answers/${answerId}`, { answerText });
  }

  deleteQuestion(questionId: string): Observable<any> {
    return this.http.delete<any>(`${API_BASE_URL}/reviews/questions/${questionId}`);
  }

  deleteAnswer(answerId: string): Observable<any> {
    return this.http.delete<any>(`${API_BASE_URL}/reviews/answers/${answerId}`);
  }

  getNotifications(isRead?: boolean, channel?: string, priority?: string, search?: string, startDate?: string, endDate?: string, sortBy?: string, page = 1): Observable<PagedResult<Notification>> {
    const params: any = { page, pageSize: 20 };
    if (isRead !== undefined) params.isRead = isRead;
    if (channel) params.channel = channel;
    if (priority) params.priority = priority;
    if (search) params.search = search;
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (sortBy) params.sortBy = sortBy;
    return this.http.get<PagedResult<Notification>>(`${API_BASE_URL}/notifications`, { params });
  }

  getNotificationById(id: string): Observable<Notification> {
    return this.http.get<Notification>(`${API_BASE_URL}/notifications/${id}`);
  }

  getUnreadNotificationsCount(): Observable<number> {
    return this.http.get<number>(`${API_BASE_URL}/notifications/unread-count`);
  }

  markNotificationAsRead(id: string): Observable<any> {
    return this.http.put<any>(`${API_BASE_URL}/notifications/${id}/read`, {});
  }

  markAllNotificationsAsRead(): Observable<any> {
    return this.http.put<any>(`${API_BASE_URL}/notifications/read-all`, {});
  }

  deleteNotification(id: string): Observable<any> {
    return this.http.delete<any>(`${API_BASE_URL}/notifications/${id}`);
  }

  sendManualNotification(request: { userId: string, title: string, message: string, notificationType: string, channel: string, priority: string }): Observable<Notification> {
    return this.http.post<Notification>(`${API_BASE_URL}/notifications/send`, request);
  }

  broadcastNotification(request: { title: string, message: string, notificationType: string, channel: string, priority: string }): Observable<any> {
    return this.http.post<any>(`${API_BASE_URL}/notifications/broadcast`, request);
  }

  getNotificationLogs(notificationId?: string, page = 1): Observable<PagedResult<NotificationLog>> {
    const params: any = { page, pageSize: 20 };
    if (notificationId) params.notificationId = notificationId;
    return this.http.get<PagedResult<NotificationLog>>(`${API_BASE_URL}/notifications/logs`, { params });
  }

  getNotificationTemplates(): Observable<NotificationTemplate[]> {
    return this.http.get<NotificationTemplate[]>(`${API_BASE_URL}/notifications/templates`);
  }

  createNotificationTemplate(request: { code: string, name: string, subject: string, body: string, channel: string }): Observable<NotificationTemplate> {
    return this.http.post<NotificationTemplate>(`${API_BASE_URL}/notifications/templates`, request);
  }

  updateNotificationTemplate(code: string, request: { name: string, subject: string, body: string, channel: string }): Observable<NotificationTemplate> {
    return this.http.put<NotificationTemplate>(`${API_BASE_URL}/notifications/templates/${code}`, request);
  }

  getReportsDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${API_BASE_URL}/reports/dashboard`);
  }

  getSalesReport(startDate?: string, endDate?: string, categoryId?: string, brandId?: string): Observable<SalesReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (categoryId) params.categoryId = categoryId;
    if (brandId) params.brandId = brandId;
    return this.http.get<SalesReport>(`${API_BASE_URL}/reports/sales`, { params });
  }

  getRevenueReport(startDate?: string, endDate?: string): Observable<RevenueReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<RevenueReport>(`${API_BASE_URL}/reports/revenue`, { params });
  }

  getOrdersReport(startDate?: string, endDate?: string, status?: string): Observable<OrdersReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (status) params.status = status;
    return this.http.get<OrdersReport>(`${API_BASE_URL}/reports/orders`, { params });
  }

  getCustomerReport(startDate?: string, endDate?: string): Observable<CustomerReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<CustomerReport>(`${API_BASE_URL}/reports/customers`, { params });
  }

  getProductReport(startDate?: string, endDate?: string, categoryId?: string, brandId?: string): Observable<ProductReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (categoryId) params.categoryId = categoryId;
    if (brandId) params.brandId = brandId;
    return this.http.get<ProductReport>(`${API_BASE_URL}/reports/products`, { params });
  }

  getInventoryReport(): Observable<InventoryReport> {
    return this.http.get<InventoryReport>(`${API_BASE_URL}/reports/inventory`);
  }

  getPaymentReport(startDate?: string, endDate?: string, method?: string): Observable<PaymentReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    if (method) params.method = method;
    return this.http.get<PaymentReport>(`${API_BASE_URL}/reports/payments`, { params });
  }

  getCouponReport(startDate?: string, endDate?: string): Observable<CouponReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<CouponReport>(`${API_BASE_URL}/reports/coupons`, { params });
  }

  getReviewReport(startDate?: string, endDate?: string): Observable<ReviewReport> {
    const params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<ReviewReport>(`${API_BASE_URL}/reports/reviews`, { params });
  }

  exportReport(reportType: string, format: string, startDate?: string, endDate?: string): Observable<Blob> {
    const params: any = { reportType, format };
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get(`${API_BASE_URL}/reports/export`, { params, responseType: 'blob' });
  }

  checkoutAddAddress(request: AddAddressRequest): Observable<UserAddressDto> {
    return this.http.post<UserAddressDto>(`${API_BASE_URL}/checkout/addresses`, request);
  }

  checkoutUpdateAddress(id: string, request: AddAddressRequest): Observable<UserAddressDto> {
    return this.http.put<UserAddressDto>(`${API_BASE_URL}/checkout/addresses/${id}`, request);
  }

  checkoutDeleteAddress(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${API_BASE_URL}/checkout/addresses/${id}`);
  }

  checkoutSetDefaultAddress(id: string): Observable<boolean> {
    return this.http.put<boolean>(`${API_BASE_URL}/checkout/addresses/${id}/default`, {});
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
