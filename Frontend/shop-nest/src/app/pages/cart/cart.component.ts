import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Cart, CartItem } from '../../core/api.models';
import { ApiService } from '../../core/api.service';
import { CartService } from '../../core/cart.service';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.scss',
})
export class CartComponent implements OnInit {
  cart?: Cart;
  shippingAddress = '';
  message = '';
  errorMsg = '';
  couponCode = '';
  loading = false;
  couponLoading = false;

  constructor(
    private readonly api: ApiService,
    private readonly cartService: CartService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMsg = '';
    this.cartService.loadCart().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = 'Failed to load cart: ' + (err.error || err.message);
      }
    });
  }

  increaseQuantity(item: CartItem): void {
    if (item.quantity >= item.availableQuantity) {
      this.showError(`Cannot exceed available stock of ${item.availableQuantity} items.`);
      return;
    }
    this.cartService.increaseQuantity(item.id).subscribe({
      next: (cart) => this.cart = cart,
      error: (err) => this.showError(err.error || err.message)
    });
  }

  decreaseQuantity(item: CartItem): void {
    this.cartService.decreaseQuantity(item.id).subscribe({
      next: (cart) => this.cart = cart,
      error: (err) => this.showError(err.error || err.message)
    });
  }

  remove(itemId: string): void {
    this.cartService.removeItem(itemId).subscribe({
      next: (cart) => {
        this.cart = cart;
        this.showNotice('Item removed from cart.');
      },
      error: (err) => this.showError(err.error || err.message)
    });
  }

  moveToWishlist(item: CartItem): void {
    this.api.addWishlistItem(item.productId).subscribe({
      next: () => {
        this.showNotice(`${item.productName} moved to wishlist.`);
        this.remove(item.id);
      },
      error: (err) => this.showError('Failed to move to wishlist: ' + (err.error || err.message))
    });
  }

  clearCart(): void {
    if (!confirm('Are you sure you want to clear your shopping cart?')) return;
    this.cartService.clearCart().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.showNotice('Shopping cart cleared.');
      },
      error: (err) => this.showError(err.error || err.message)
    });
  }

  applyCoupon(): void {
    if (!this.couponCode.trim()) return;
    this.couponLoading = true;
    this.errorMsg = '';
    this.cartService.applyCoupon(this.couponCode.trim()).subscribe({
      next: (cart) => {
        this.cart = cart;
        this.couponLoading = false;
        this.showNotice('Coupon applied successfully!');
      },
      error: (err) => {
        this.couponLoading = false;
        this.showError(err.error || err.message);
      }
    });
  }

  removeCoupon(): void {
    this.cartService.removeCoupon().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.couponCode = '';
        this.showNotice('Coupon removed.');
      },
      error: (err) => this.showError(err.error || err.message)
    });
  }

  checkout(): void {
    if (!this.shippingAddress.trim()) {
      this.showError('Shipping address is required.');
      return;
    }
    this.loading = true;
    this.api.checkout(this.shippingAddress).subscribe({
      next: (order) => {
        this.loading = false;
        this.message = `Order ${order.orderNumber} placed successfully!`;
        setTimeout(() => {
          void this.router.navigateByUrl('/orders');
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.showError(err.error || err.message);
      }
    });
  }

  image(item: CartItem): string {
    return item.imageUrl || 'https://images.unsplash.com/photo-1557821552-17105176677c?auto=format&fit=crop&w=900&q=80';
  }

  showError(msg: string): void {
    this.errorMsg = msg;
    setTimeout(() => this.errorMsg = '', 4000);
  }

  showNotice(msg: string): void {
    this.message = msg;
    setTimeout(() => this.message = '', 3000);
  }
}
