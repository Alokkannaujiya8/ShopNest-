import { Injectable, signal, computed } from '@angular/core';
import { Cart } from './api.models';

@Injectable({
  providedIn: 'root'
})
export class CartStateService {
  private readonly cartSignal = signal<Cart | null>(null);

  readonly cart = this.cartSignal.asReadonly();

  readonly count = computed(() => {
    const c = this.cartSignal();
    if (!c) return 0;
    return c.items.reduce((acc, item) => acc + item.quantity, 0);
  });

  readonly totals = computed(() => {
    const c = this.cartSignal();
    if (!c) {
      return {
        subtotal: 0,
        discount: 0,
        tax: 0,
        shipping: 0,
        grandTotal: 0
      };
    }
    // Compute total metrics
    const subtotal = c.subtotal || 0;
    const discount = c.totalDiscount || 0;
    const tax = c.estimatedTax || 0;
    const shipping = c.shippingCharges || 0;
    const grandTotal = c.grandTotal || (subtotal - discount + tax + shipping);
    
    return {
      subtotal,
      discount,
      tax,
      shipping,
      grandTotal
    };
  });

  readonly appliedCoupon = computed(() => {
    const c = this.cartSignal();
    return c?.appliedCouponCode || null;
  });

  setCart(cart: Cart | null) {
    this.cartSignal.set(cart);
  }

  clear() {
    this.cartSignal.set(null);
  }
}
