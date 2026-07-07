import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-toast',
  standalone: false,
  template: `
    <div class="toast-container" aria-live="assertive" aria-atomic="true">
      <div 
        *ngFor="let toast of toastService.toasts()" 
        class="toast-card" 
        [attr.data-type]="toast.type"
        (click)="toastService.remove(toast.id)"
      >
        <span class="toast-icon">
          <ng-container [ngSwitch]="toast.type">
            <span *ngSwitchCase="'success'">✅</span>
            <span *ngSwitchCase="'error'">❌</span>
            <span *ngSwitchCase="'warning'">⚠️</span>
            <span *ngSwitchCase="'info'">ℹ️</span>
          </ng-container>
        </span>
        <span class="toast-message">{{ toast.message }}</span>
        <button class="toast-close" type="button" aria-label="Close alert">×</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: 20px;
      right: 20px;
      display: flex;
      flex-direction: column;
      gap: 10px;
      z-index: 999999;
      max-width: 350px;
      width: 100%;
    }
    .toast-card {
      display: grid;
      grid-template-columns: auto 1fr auto;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      background: var(--panel, #ffffff);
      border-left: 5px solid var(--accent, #0f766e);
      border-radius: 8px;
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.15);
      cursor: pointer;
      color: var(--ink, #18201c);
      animation: slideIn 0.25s ease-out;
      transition: transform 0.2s ease, opacity 0.2s ease;
    }
    .toast-card:hover {
      transform: translateY(-2px);
    }
    .toast-card[data-type="success"] { border-left-color: #10b981; }
    .toast-card[data-type="error"] { border-left-color: var(--danger, #b91c1c); }
    .toast-card[data-type="warning"] { border-left-color: var(--warn, #b45309); }
    .toast-card[data-type="info"] { border-left-color: #3b82f6; }

    .toast-icon {
      font-size: 1.2rem;
    }
    .toast-message {
      font-size: 0.9rem;
      font-weight: 500;
      line-height: 1.4;
    }
    .toast-close {
      background: transparent;
      border: 0;
      font-size: 1.3rem;
      cursor: pointer;
      color: var(--muted, #627069);
      padding: 0 4px;
    }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class ToastComponent {
  readonly toastService = inject(ToastService);
}
