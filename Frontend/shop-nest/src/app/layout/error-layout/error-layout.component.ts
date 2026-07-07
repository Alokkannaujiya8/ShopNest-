import { Component } from '@angular/core';

@Component({
  selector: 'app-error-layout',
  standalone: false,
  template: `
    <div class="error-layout-container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .error-layout-container {
      display: grid;
      place-items: center;
      min-height: 100vh;
      background: var(--bg, #f6f7f4);
    }
  `]
})
export class ErrorLayoutComponent {}
