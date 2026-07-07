import { Component, inject } from '@angular/core';
import { LoadingService } from '../../../core/loading.service';

@Component({
  selector: 'app-loader',
  standalone: false,
  template: `
    <div class="loader-overlay" *ngIf="loader.isLoading()">
      <div class="loader-spinner" role="status" aria-live="polite">
        <span class="sr-only">Loading...</span>
      </div>
    </div>
  `,
  styles: [`
    .loader-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      display: grid;
      place-items: center;
      z-index: 99999;
      backdrop-filter: blur(2px);
    }
    .loader-spinner {
      width: 48px;
      height: 48px;
      border: 5px solid rgba(255, 255, 255, 0.3);
      border-radius: 50%;
      border-top-color: var(--accent, #0f766e);
      animation: spin 1s linear infinite;
    }
    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      border: 0;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoaderComponent {
  readonly loader = inject(LoadingService);
}
