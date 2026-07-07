import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardService, DashboardSummary } from '../../core/dashboard.service';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-customer-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class CustomerDashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  readonly api = inject(ApiService);

  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading.set(true);
    this.error.set(false);

    this.dashboardService.getDashboardSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
        this.toast.error('Failed to load dashboard metrics.');
      }
    });
  }

  navigate(path: string): void {
    void this.router.navigateByUrl(path);
  }
}
