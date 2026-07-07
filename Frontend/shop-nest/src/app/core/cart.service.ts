import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { CartStateService } from './cart-state.service';
import { Cart, Product } from './api.models';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly api = inject(ApiService);
  private readonly state = inject(CartStateService);

  loadCart(): Observable<Cart> {
    return this.api.cart().pipe(
      tap(cart => {
        this.state.setCart(cart);
        // Sync original cartItemCount signal in api service
        this.api.cartItemCount.set(cart.items.reduce((acc, item) => acc + item.quantity, 0));
      })
    );
  }

  addToCart(productId: string, quantity: number = 1): Observable<Cart> {
    return this.api.addToCart(productId, quantity).pipe(
      tap(cart => {
        this.state.setCart(cart);
        this.api.cartItemCount.set(cart.items.reduce((acc, item) => acc + item.quantity, 0));
      })
    );
  }

  increaseQuantity(itemId: string): Observable<Cart> {
    return this.api.increaseCartItemQuantity(itemId).pipe(
      tap(cart => {
        this.state.setCart(cart);
        this.api.cartItemCount.set(cart.items.reduce((acc, item) => acc + item.quantity, 0));
      })
    );
  }

  decreaseQuantity(itemId: string): Observable<Cart> {
    return this.api.decreaseCartItemQuantity(itemId).pipe(
      tap(cart => {
        this.state.setCart(cart);
        this.api.cartItemCount.set(cart.items.reduce((acc, item) => acc + item.quantity, 0));
      })
    );
  }

  removeItem(itemId: string): Observable<Cart> {
    return this.api.removeCartItem(itemId).pipe(
      tap(cart => {
        this.state.setCart(cart);
        this.api.cartItemCount.set(cart.items.reduce((acc, item) => acc + item.quantity, 0));
      })
    );
  }

  clearCart(): Observable<Cart> {
    return this.api.clearCart().pipe(
      tap(cart => {
        this.state.setCart(cart);
        this.api.cartItemCount.set(0);
      })
    );
  }

  applyCoupon(couponCode: string): Observable<Cart> {
    return this.api.applyCoupon(couponCode).pipe(
      tap(cart => {
        this.state.setCart(cart);
      })
    );
  }

  removeCoupon(): Observable<Cart> {
    return this.api.removeCoupon().pipe(
      tap(cart => {
        this.state.setCart(cart);
      })
    );
  }
}
