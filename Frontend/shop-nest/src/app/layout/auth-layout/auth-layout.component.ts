import { Component } from '@angular/core';

@Component({
  selector: 'app-auth-layout',
  standalone: false,
  template: `
    <div class="auth-layout-container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .auth-layout-container {
      display: grid;
      place-items: center;
      min-height: 100vh;
      background: var(--bg, #f6f7f4);
    }
  `]
})
export class AuthLayoutComponent {}
