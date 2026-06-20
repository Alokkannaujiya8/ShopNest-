import { Component, OnInit, inject, signal } from '@angular/core';
import { ApiService } from './core/api.service';
import { SignalrService } from './core/signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  readonly signalr = inject(SignalrService);
  
  readonly isSidebarOpen = signal(false);
  readonly isDarkTheme = signal(false);

  constructor(readonly api: ApiService) {}

  ngOnInit(): void {
    this.signalr.init();
    
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
