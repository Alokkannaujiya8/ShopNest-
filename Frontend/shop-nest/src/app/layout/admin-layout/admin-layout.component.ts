import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';

@Component({
  selector: 'app-admin-layout',
  standalone: false,
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent {
  readonly api = inject(ApiService);
  readonly signalr = inject(SignalrService);
  private readonly router = inject(Router);

  readonly isSidebarOpen = signal(false);
  readonly isDarkTheme = signal(false);

  constructor() {
    const savedTheme = localStorage.getItem('shopnest.theme') || 'light';
    this.isDarkTheme.set(savedTheme === 'dark');
    this.applyTheme(savedTheme === 'dark');
  }

  logout(): void {
    this.api.logout();
    this.isSidebarOpen.set(false);
    void this.signalr.disconnect().then(() => {
      this.signalr.init();
    });
    void this.router.navigate(['/auth/login']);
  }

  toggleTheme(): void {
    const nextTheme = !this.isDarkTheme();
    this.isDarkTheme.set(nextTheme);
    localStorage.setItem('shopnest.theme', nextTheme ? 'dark' : 'light');
    this.applyTheme(nextTheme);
  }

  private applyTheme(isDark: boolean): void {
    const doc = document.documentElement;
    if (isDark) {
      doc.setAttribute('data-theme', 'dark');
    } else {
      doc.removeAttribute('data-theme');
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen.update(v => !v);
  }

  closeSidebar(): void {
    this.isSidebarOpen.set(false);
  }
}
