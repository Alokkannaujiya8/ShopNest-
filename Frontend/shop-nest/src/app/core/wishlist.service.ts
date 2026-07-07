import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { WishlistStateService } from './wishlist-state.service';
import { WishlistItem, PagedResult, WishlistSearchRequest } from './api.models';

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private readonly api = inject(ApiService);
  private readonly state = inject(WishlistStateService);

  loadWishlist(): Observable<PagedResult<WishlistItem>> {
    return this.api.getWishlistItems({ page: 1, pageSize: 100 }).pipe(
      tap(res => {
        this.state.setItems(res.items);
      })
    );
  }

  addToWishlist(productId: string): Observable<WishlistItem> {
    return this.api.addWishlistItem(productId).pipe(
      tap(item => {
        this.state.addItem(item);
      })
    );
  }

  removeFromWishlist(productId: string): Observable<boolean> {
    return this.api.removeWishlistItem(productId).pipe(
      tap(success => {
        if (success) {
          this.state.removeItem(productId);
        }
      })
    );
  }

  clearWishlist(): Observable<boolean> {
    return this.api.clearWishlistItems().pipe(
      tap(success => {
        if (success) {
          this.state.clear();
        }
      })
    );
  }
}
