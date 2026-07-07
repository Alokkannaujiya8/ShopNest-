import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/guards';
import { AdminComponent } from './pages/admin/admin.component';
import { CartComponent } from './pages/cart/cart.component';
import { CatalogComponent } from './pages/catalog/catalog.component';
import { OrdersComponent } from './pages/orders/orders.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { HomeComponent } from './pages/catalog/home.component';
import { CustomerDashboardComponent } from './pages/profile/dashboard.component';
import { CategoryListComponent } from './pages/category/category-list.component';
import { CategoryDetailsComponent } from './pages/category/category-details.component';
import { BrandListComponent } from './pages/brand/brand-list.component';
import { BrandDetailsComponent } from './pages/brand/brand-details.component';

import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';
import { VerifyOtpComponent } from './pages/auth/verify-otp/verify-otp.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password.component';

import { AdminUsersComponent } from './pages/admin/users/admin-users.component';
import { AdminRolesComponent } from './pages/admin/roles/admin-roles.component';
import { AdminCategoriesComponent } from './pages/admin/categories/admin-categories.component';
import { AdminProductsComponent } from './pages/admin/products/admin-products.component';
import { AdminProductEditComponent } from './pages/admin/products/admin-product-edit.component';
import { AdminInventoryComponent } from './pages/admin/inventory/admin-inventory.component';
import { WishlistComponent } from './pages/wishlist/wishlist.component';
import { ProductDetailsComponent } from './pages/catalog/product-details.component';
import { CheckoutComponent } from './pages/checkout/checkout.component';
import { OrderSuccessComponent } from './pages/checkout/order-success.component';
import { OrderFailureComponent } from './pages/checkout/order-failure.component';
import { PaymentComponent } from './pages/payment/payment.component';
import { ShippingComponent } from './pages/shipping/shipping.component';
import { AdminReviewsComponent } from './pages/admin/reviews/admin-reviews.component';
import { AdminNotificationsComponent } from './pages/admin/notifications/admin-notifications.component';
import { NotificationsCenterComponent } from './pages/notifications/notifications-center.component';
import { NotificationSettingsComponent } from './pages/notifications/notification-settings.component';
import { ReportsDashboardComponent } from './pages/admin/reports/reports-dashboard.component';
import { AdminAuditLogsComponent } from './pages/admin/audit-logs/admin-audit-logs.component';
 
import { AuthLayoutComponent } from './layout/auth-layout/auth-layout.component';
import { CustomerLayoutComponent } from './layout/customer-layout/customer-layout.component';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';

const routes: Routes = [
  // Customer / Storefront Layout Routes
  {
    path: '',
    component: CustomerLayoutComponent,
    children: [
      { path: '', component: HomeComponent, data: { title: 'Home Page' } },
      { path: 'catalog', component: CatalogComponent, data: { title: 'Catalog & Shop' } },
      { path: 'catalog/product/:slug', component: ProductDetailsComponent, data: { breadcrumb: 'Product Details' } },
      { path: 'products', component: CatalogComponent, data: { title: 'Products List', breadcrumb: 'Products' } },
      { path: 'products/:id', component: ProductDetailsComponent, data: { title: 'Product Details', breadcrumb: 'Details' } },
      { path: 'products/category/:categoryId', component: CatalogComponent, data: { title: 'Category Catalog', breadcrumb: 'Category' } },
      { path: 'products/brand/:brandId', component: CatalogComponent, data: { title: 'Brand Catalog', breadcrumb: 'Brand' } },
      { path: 'search', component: CatalogComponent, data: { title: 'Search', breadcrumb: 'Search' } },
      { path: 'search/results', component: CatalogComponent, data: { title: 'Search Results', breadcrumb: 'Results' } },
      { path: 'cart', component: CartComponent, canActivate: [authGuard], data: { title: 'Shopping Cart', breadcrumb: 'Cart' } },
      { path: 'checkout', component: CheckoutComponent, canActivate: [authGuard], data: { title: 'Checkout Step', breadcrumb: 'Checkout' } },
      { path: 'order-success', component: OrderSuccessComponent, canActivate: [authGuard], data: { title: 'Order Success', breadcrumb: 'Confirmed' } },
      { path: 'order-failure', component: OrderFailureComponent, canActivate: [authGuard], data: { title: 'Order Failure', breadcrumb: 'Failed' } },
      { path: 'payment', component: PaymentComponent, canActivate: [authGuard], data: { title: 'Order Payment', breadcrumb: 'Payment' } },
      { path: 'wishlist', component: WishlistComponent, canActivate: [authGuard], data: { title: 'My Wishlist', breadcrumb: 'Wishlist' } },
      { path: 'orders', component: OrdersComponent, canActivate: [authGuard], data: { title: 'Orders History', breadcrumb: 'My Orders' } },
      { path: 'orders/:orderId', component: OrdersComponent, canActivate: [authGuard], data: { title: 'Order Details', breadcrumb: 'Details' } },
      { path: 'orders/:orderId/tracking', component: OrdersComponent, canActivate: [authGuard], data: { title: 'Order Tracking', breadcrumb: 'Tracking' } },
      { path: 'shipping', component: ShippingComponent, canActivate: [authGuard], data: { title: 'Shipping Status', breadcrumb: 'Shipping' } },
      { path: 'profile', component: CustomerDashboardComponent, canActivate: [authGuard], data: { title: 'My Dashboard', breadcrumb: 'Dashboard' } },
      { path: 'profile/edit', component: ProfileComponent, canActivate: [authGuard], data: { title: 'Edit Profile', breadcrumb: 'Profile Details' } },
      { path: 'addresses', component: ProfileComponent, canActivate: [authGuard], data: { title: 'Manage Addresses', breadcrumb: 'Saved Addresses' } },
      { path: 'change-password', component: ProfileComponent, canActivate: [authGuard], data: { title: 'Change Password', breadcrumb: 'Password Security' } },
      { path: 'my-reviews', component: ProfileComponent, canActivate: [authGuard], data: { title: 'My Reviews', breadcrumb: 'My Reviews' } },
      { path: 'account/settings', component: ProfileComponent, canActivate: [authGuard], data: { title: 'Account Settings', breadcrumb: 'Settings' } },
      { path: 'notifications', component: NotificationsCenterComponent, canActivate: [authGuard], data: { title: 'Notification Center', breadcrumb: 'Notifications' } },
      { path: 'notification-settings', component: NotificationSettingsComponent, canActivate: [authGuard], data: { title: 'Notification Settings', breadcrumb: 'Settings' } },
      { path: 'categories', component: CategoryListComponent, data: { title: 'Categories', breadcrumb: 'Categories' } },
      { path: 'categories/:id', component: CategoryDetailsComponent, data: { title: 'Category Details', breadcrumb: 'Category Details' } },
      { path: 'brands', component: BrandListComponent, data: { title: 'Brands', breadcrumb: 'Brands' } },
      { path: 'brands/:id', component: BrandDetailsComponent, data: { title: 'Brand Details', breadcrumb: 'Brand Details' } }
    ]
  },

  // Auth Layout Routes
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent, canActivate: [guestGuard], data: { title: 'Sign In' } },
      { path: 'register', component: RegisterComponent, canActivate: [guestGuard], data: { title: 'Create Account' } },
      { path: 'verify-otp', component: VerifyOtpComponent, canActivate: [guestGuard], data: { title: 'Verify OTP' } },
      { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard], data: { title: 'Forgot Password' } },
      { path: 'reset-password', component: ResetPasswordComponent, canActivate: [guestGuard], data: { title: 'Reset Password' } }
    ]
  },

  // Admin Layout Routes
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [authGuard, adminGuard],
    children: [
      { path: '', component: AdminComponent, data: { title: 'Admin Dashboard' } },
      { path: 'dashboard', component: AdminComponent, data: { title: 'Admin Dashboard', breadcrumb: 'Dashboard' } },
      { path: 'users', component: AdminUsersComponent, data: { title: 'Users Management', breadcrumb: 'Users' } },
      { path: 'roles', component: AdminRolesComponent, data: { title: 'Roles Management', breadcrumb: 'Roles' } },
      { path: 'categories', component: AdminCategoriesComponent, data: { title: 'Category Catalog', breadcrumb: 'Categories' } },
      { path: 'categories/create', component: AdminCategoriesComponent, data: { title: 'Create Category', breadcrumb: 'New Category' } },
      { path: 'categories/:id', component: AdminCategoriesComponent, data: { title: 'Category Details', breadcrumb: 'Category' } },
      { path: 'categories/:id/edit', component: AdminCategoriesComponent, data: { title: 'Edit Category', breadcrumb: 'Edit Category' } },
      { path: 'brands', component: AdminCategoriesComponent, data: { title: 'Brand Catalog', breadcrumb: 'Brands' } },
      { path: 'brands/create', component: AdminCategoriesComponent, data: { title: 'Create Brand', breadcrumb: 'New Brand' } },
      { path: 'brands/:id', component: AdminCategoriesComponent, data: { title: 'Brand Details', breadcrumb: 'Brand' } },
      { path: 'brands/:id/edit', component: AdminCategoriesComponent, data: { title: 'Edit Brand', breadcrumb: 'Edit Brand' } },
      { path: 'products', component: AdminProductsComponent, data: { title: 'Product Inventory', breadcrumb: 'Products' } },
      { path: 'products/new', component: AdminProductEditComponent, data: { title: 'Add Product', breadcrumb: 'New Product' } },
      { path: 'products/create', component: AdminProductEditComponent, data: { title: 'Add Product', breadcrumb: 'New Product' } },
      { path: 'products/edit/:id', component: AdminProductEditComponent, data: { title: 'Edit Product', breadcrumb: 'Modify Product' } },
      { path: 'products/:id/edit', component: AdminProductEditComponent, data: { title: 'Edit Product', breadcrumb: 'Modify Product' } },
      { path: 'products/:id', component: ProductDetailsComponent, data: { title: 'Product Details', breadcrumb: 'Details' } },
      { path: 'inventory', component: AdminInventoryComponent, data: { title: 'Warehouse Stock', breadcrumb: 'Inventory' } },
      { path: 'inventory/:productId', component: AdminInventoryComponent, data: { title: 'Warehouse Stock Details', breadcrumb: 'Details' } },
      { path: 'orders', component: AdminComponent, data: { title: 'Order Management', breadcrumb: 'Orders' } },
      { path: 'orders/:orderId', component: AdminComponent, data: { title: 'Order Details', breadcrumb: 'Details' } },
      { path: 'shipping', component: AdminComponent, data: { title: 'Shipping Status', breadcrumb: 'Shipping' } },
      { path: 'shipping/:orderId', component: AdminComponent, data: { title: 'Shipping Tracking', breadcrumb: 'Tracking' } },
      { path: 'payments', component: ReportsDashboardComponent, data: { title: 'Payment Reports', breadcrumb: 'Payments' } },
      { path: 'payments/:paymentId', component: ReportsDashboardComponent, data: { title: 'Payment Details', breadcrumb: 'Details' } },
      { path: 'reviews', component: AdminReviewsComponent, data: { title: 'Reviews Moderation', breadcrumb: 'Reviews' } },
      { path: 'notifications', component: AdminNotificationsComponent, data: { title: 'System Notifications', breadcrumb: 'Notifications' } },
      { path: 'reports', component: ReportsDashboardComponent, data: { title: 'Reports & Business Intelligence', breadcrumb: 'Reports' } },
      { path: 'reports/sales', component: ReportsDashboardComponent, data: { title: 'Sales Reports', breadcrumb: 'Sales' } },
      { path: 'reports/revenue', component: ReportsDashboardComponent, data: { title: 'Revenue Reports', breadcrumb: 'Revenue' } },
      { path: 'reports/products', component: ReportsDashboardComponent, data: { title: 'Product Reports', breadcrumb: 'Products' } },
      { path: 'reports/inventory', component: ReportsDashboardComponent, data: { title: 'Inventory Reports', breadcrumb: 'Inventory' } },
      { path: 'reports/customers', component: ReportsDashboardComponent, data: { title: 'Customer Reports', breadcrumb: 'Customers' } },
      { path: 'reports/orders', component: ReportsDashboardComponent, data: { title: 'Order Reports', breadcrumb: 'Orders' } },
      { path: 'audit-logs', component: AdminAuditLogsComponent, data: { title: 'Activity Audit Logs', breadcrumb: 'Audit Logs' } },
      { path: 'audit-logs/:id', component: AdminAuditLogsComponent, data: { title: 'Audit Log Details', breadcrumb: 'Details' } }
    ]
  },

  // Legacy Redirect
  { path: 'login', redirectTo: 'auth/login', pathMatch: 'full' },
  
  // Fallbacks
  { path: '**', redirectTo: 'catalog' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }



