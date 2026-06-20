import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Cart } from '../../core/api.models';
import { ApiService } from '../../core/api.service';

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

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.cart().subscribe((cart) => (this.cart = cart));
  }

  update(itemId: string, quantity: number): void {
    this.api.updateCartItem(itemId, quantity).subscribe((cart) => (this.cart = cart));
  }

  remove(itemId: string): void {
    this.api.removeCartItem(itemId).subscribe((cart) => (this.cart = cart));
  }

  checkout(): void {
    this.api.checkout(this.shippingAddress).subscribe((order) => {
      this.message = `Order ${order.orderNumber} created.`;
      void this.router.navigateByUrl('/orders');
    });
  }
}
