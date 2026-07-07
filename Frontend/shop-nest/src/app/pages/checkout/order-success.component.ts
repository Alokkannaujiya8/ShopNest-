import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-order-success',
  standalone: false,
  template: `
    <section class="order-success-container">
      <div class="panel success-card">
        <div class="success-icon">🎉</div>
        <h1>Order Placed Successfully!</h1>
        <p class="subtitle">Thank you for your order. We are preparing it for shipment.</p>

        <div class="order-info-box" *ngIf="orderNumber()">
          <div class="info-row">
            <span class="label">Order Number:</span>
            <span class="value font-code">#{{ orderNumber() }}</span>
          </div>
          <div class="info-row" *ngIf="estimatedDelivery()">
            <span class="label">Estimated Delivery:</span>
            <span class="value">{{ estimatedDelivery() | date:'mediumDate' }}</span>
          </div>
        </div>

        <div class="actions">
          <a routerLink="/orders" class="primary button-link">View Orders History</a>
          <a routerLink="/catalog" class="outline-btn">Continue Shopping</a>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .order-success-container {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 3rem 1rem;
    }
    .success-card {
      max-width: 500px;
      width: 100%;
      text-align: center;
      padding: 3rem 2rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
    }
    .success-icon {
      font-size: 4rem;
      background: var(--soft);
      width: 100px;
      height: 100px;
      border-radius: 50%;
      display: grid;
      place-items: center;
    }
    h1 {
      font-size: 1.75rem;
      font-weight: 850;
      color: var(--ink);
    }
    .subtitle {
      color: var(--muted);
      font-size: 0.95rem;
    }
    .order-info-box {
      background: var(--soft);
      border-radius: 8px;
      width: 100%;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .info-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.9rem;
    }
    .info-row .label {
      color: var(--muted);
      font-weight: 600;
    }
    .info-row .value {
      color: var(--ink);
      font-weight: 750;
    }
    .font-code {
      font-family: monospace;
      font-size: 1rem;
    }
    .actions {
      display: flex;
      gap: 12px;
      width: 100%;
      margin-top: 1rem;
    }
    .actions a, .actions button {
      flex: 1;
      text-align: center;
    }
    .button-link {
      padding: 10px 16px;
      border-radius: 6px;
      text-decoration: none;
      font-weight: 700;
      display: inline-block;
    }
    .outline-btn {
      padding: 10px 16px;
      border: 1px solid var(--line);
      border-radius: 6px;
      text-decoration: none;
      color: var(--muted);
      font-weight: 700;
      display: inline-block;
      transition: background-color 0.2s, color 0.2s;
    }
    .outline-btn:hover {
      background: var(--soft);
      color: var(--ink);
    }
  `]
})
export class OrderSuccessComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);

  readonly orderNumber = signal<string | null>(null);
  readonly estimatedDelivery = signal<string | null>(null);

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.orderNumber.set(params.get('orderNumber'));
      this.estimatedDelivery.set(params.get('estimatedDelivery'));
    });
  }
}
