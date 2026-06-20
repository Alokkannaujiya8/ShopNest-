import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { Order } from '../../core/api.models';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';

@Component({
  selector: 'app-orders',
  standalone: false,
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss',
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  paymentMessage = '';
  activeSession?: { paymentId: string; providerOrderId: string; orderNumber: string };
  private sub?: Subscription;

  constructor(
    private readonly api: ApiService,
    readonly signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.loadOrders();
    this.sub = this.signalr.orderStatusChanged$.subscribe(() => {
      this.loadOrders();
    });
  }

  loadOrders(): void {
    this.api.myOrders().subscribe((result) => (this.orders = result.items));
  }

  pay(order: Order): void {
    this.api.createPayment(order.id).subscribe((session) => {
      this.activeSession = {
        paymentId: session.paymentId,
        providerOrderId: session.providerOrderId,
        orderNumber: order.orderNumber
      };
      this.paymentMessage = `Payment session ready for Order ${order.orderNumber}.`;
    });
  }

  simulateSuccess(): void {
    if (!this.activeSession) return;
    this.api.completePayment(this.activeSession.paymentId, 'Succeeded').subscribe({
      next: () => {
        this.paymentMessage = `Simulated Payment Succeeded for Order ${this.activeSession?.orderNumber}!`;
        this.activeSession = undefined;
        this.loadOrders();
      },
      error: () => {
        this.paymentMessage = 'Simulation failed.';
      }
    });
  }

  simulateFailure(): void {
    if (!this.activeSession) return;
    this.api.completePayment(this.activeSession.paymentId, 'Failed').subscribe({
      next: () => {
        this.paymentMessage = `Simulated Payment Failed for Order ${this.activeSession?.orderNumber}.`;
        this.activeSession = undefined;
        this.loadOrders();
      },
      error: () => {
        this.paymentMessage = 'Simulation failed.';
      }
    });
  }
}
