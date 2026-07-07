import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-order-failure',
  standalone: false,
  template: `
    <section class="order-failure-container">
      <div class="panel failure-card">
        <div class="failure-icon">❌</div>
        <h1>Order Placement Failed</h1>
        <p class="subtitle">There was an issue processing your transaction. Please verify details and try again.</p>

        <div class="error-info-box" *ngIf="errorMessage()">
          <span class="label">Reason:</span>
          <span class="value">{{ errorMessage() }}</span>
        </div>

        <div class="actions">
          <a routerLink="/checkout" class="primary button-link">Retry Checkout</a>
          <a routerLink="/cart" class="outline-btn">Return to Cart</a>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .order-failure-container {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 3rem 1rem;
    }
    .failure-card {
      max-width: 500px;
      width: 100%;
      text-align: center;
      padding: 3rem 2rem;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
    }
    .failure-icon {
      font-size: 4rem;
      background: rgba(239, 68, 68, 0.1);
      color: #ef4444;
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
    .error-info-box {
      background: var(--soft);
      border-radius: 8px;
      width: 100%;
      padding: 1.25rem;
      display: flex;
      gap: 8px;
      justify-content: center;
      font-size: 0.9rem;
    }
    .error-info-box .label {
      color: #ef4444;
      font-weight: 700;
    }
    .error-info-box .value {
      color: var(--ink);
      font-weight: 600;
    }
    .actions {
      display: flex;
      gap: 12px;
      width: 100%;
      margin-top: 1rem;
    }
    .actions a {
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
export class OrderFailureComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);

  readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      this.errorMessage.set(params.get('error') || 'Payment authorization failed.');
    });
  }
}
