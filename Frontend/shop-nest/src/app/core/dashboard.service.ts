import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, map } from 'rxjs';
import { ApiService } from './api.service';
import { Order, Product, UserAddressDto, UserProfileDto } from './api.models';

export interface DashboardSummary {
  profile: UserProfileDto;
  totalOrdersCount: number;
  pendingOrdersCount: number;
  deliveredOrdersCount: number;
  wishlistCount: number;
  addressesCount: number;
  recentOrders: Order[];
  wishlistProducts: Product[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly api = inject(ApiService);

  getDashboardSummary(): Observable<DashboardSummary> {
    return forkJoin({
      profileRes: this.api.profile(),
      ordersRes: this.api.profileOrders(),
      wishlistRes: this.api.wishlist(),
      addressesRes: this.api.addresses()
    }).pipe(
      map(({ profileRes, ordersRes, wishlistRes, addressesRes }) => {
        const profile = profileRes.data || {} as UserProfileDto;
        const orders = ordersRes.data || [];
        const wishlistProducts = wishlistRes.data || [];
        const addresses = addressesRes.data || [];

        const pendingOrders = orders.filter(o => o.status === 'Pending' || o.status === 'Shipped');
        const deliveredOrders = orders.filter(o => o.status === 'Delivered');

        return {
          profile,
          totalOrdersCount: orders.length,
          pendingOrdersCount: pendingOrders.length,
          deliveredOrdersCount: deliveredOrders.length,
          wishlistCount: wishlistProducts.length,
          addressesCount: addresses.length,
          recentOrders: orders.slice(0, 5),
          wishlistProducts: wishlistProducts.slice(0, 4)
        };
      })
    );
  }
}
