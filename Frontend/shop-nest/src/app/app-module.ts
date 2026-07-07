import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { AdminComponent } from './pages/admin/admin.component';
import { CartComponent } from './pages/cart/cart.component';
import { CatalogComponent } from './pages/catalog/catalog.component';
import { CategoryListComponent } from './pages/category/category-list.component';
import { CategoryDetailsComponent } from './pages/category/category-details.component';
import { BrandListComponent } from './pages/brand/brand-list.component';
import { BrandDetailsComponent } from './pages/brand/brand-details.component';
import { HomeComponent } from './pages/catalog/home.component';
import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';
import { VerifyOtpComponent } from './pages/auth/verify-otp/verify-otp.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password.component';
import { OrdersComponent } from './pages/orders/orders.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { CustomerDashboardComponent } from './pages/profile/dashboard.component';
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
import { NotificationDrawerComponent } from './pages/notifications/notification-drawer.component';
import { NotificationsCenterComponent } from './pages/notifications/notifications-center.component';
import { NotificationSettingsComponent } from './pages/notifications/notification-settings.component';
import { AdminNotificationsComponent } from './pages/admin/notifications/admin-notifications.component';
import { ReportsDashboardComponent } from './pages/admin/reports/reports-dashboard.component';
import { SvgChartComponent } from './pages/admin/reports/svg-chart.component';
import { AdminAuditLogsComponent } from './pages/admin/audit-logs/admin-audit-logs.component';
import { authInterceptor } from './core/auth.interceptor';
import { AuthLayoutComponent } from './layout/auth-layout/auth-layout.component';
import { ErrorLayoutComponent } from './layout/error-layout/error-layout.component';
import { CustomerLayoutComponent } from './layout/customer-layout/customer-layout.component';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';
import { SearchBarComponent } from './shared/components/search-bar/search-bar.component';
import { BreadcrumbComponent } from './shared/components/breadcrumb/breadcrumb.component';
import { loadingInterceptor } from './core/loading.interceptor';
import { errorInterceptor } from './core/error.interceptor';
import { LoaderComponent } from './shared/components/loader/loader.component';
import { ToastComponent } from './shared/components/toast/toast.component';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { authReducer, cartReducer, productsReducer, AuthEffects, CartEffects, ProductsEffects } from './state/app.state';

@NgModule({
  declarations: [
    App,
    AdminComponent,
    AdminUsersComponent,
    AdminRolesComponent,
    AdminCategoriesComponent,
    AdminProductsComponent,
    AdminProductEditComponent,
    AdminInventoryComponent,
    CartComponent,
    CatalogComponent,
    WishlistComponent,
    ProductDetailsComponent,
    CheckoutComponent,
    OrderSuccessComponent,
    OrderFailureComponent,
    PaymentComponent,
    LoginComponent,
    RegisterComponent,
    VerifyOtpComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    OrdersComponent,
    ProfileComponent,
    CustomerDashboardComponent,
    HomeComponent,
    CategoryListComponent,
    CategoryDetailsComponent,
    BrandListComponent,
    BrandDetailsComponent,
    ShippingComponent,
    AdminReviewsComponent,
    NotificationDrawerComponent,
    NotificationsCenterComponent,
    NotificationSettingsComponent,
    AdminNotificationsComponent,
    ReportsDashboardComponent,
    SvgChartComponent,
    AdminAuditLogsComponent,
    LoaderComponent,
    ToastComponent,
    AuthLayoutComponent,
    ErrorLayoutComponent,
    CustomerLayoutComponent,
    AdminLayoutComponent,
    SearchBarComponent,
    BreadcrumbComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    StoreModule.forRoot({ auth: authReducer, cart: cartReducer, products: productsReducer }),
    EffectsModule.forRoot([AuthEffects, CartEffects, ProductsEffects])
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor, loadingInterceptor, errorInterceptor]))
  ],
  bootstrap: [App]
})
export class AppModule { }
