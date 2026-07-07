import { Component, OnInit, inject, signal } from '@angular/core';
import { BrandService, Brand } from '../../core/brand.service';

@Component({
  selector: 'app-brand-list',
  standalone: false,
  template: `
    <section class="brand-list-container">
      <div class="section-header">
        <h2>Manufacturing Brands</h2>
        <p>Explore high-quality products designed by our industry partners</p>
      </div>

      <!-- Loading skeleton -->
      <div class="loading-state" *ngIf="loading()">
        <div class="brands-grid">
          <div class="skeleton-card" *ngFor="let item of [1, 2, 3, 4]"></div>
        </div>
      </div>

      <!-- Error / Empty state -->
      <div class="error-state" *ngIf="!loading() && error()">
        <p>Failed to load designer brands.</p>
        <button class="primary" (click)="loadBrands()">Retry</button>
      </div>

      <div class="empty-state" *ngIf="!loading() && !error() && brands().length === 0">
        No partner brands found in the catalog.
      </div>

      <!-- Brands grid -->
      <div class="brands-grid" *ngIf="!loading() && !error() && brands().length > 0">
        <div *ngFor="let b of brands(); trackBy: trackById" class="panel brand-card">
          <div class="brand-logo-avatar">{{ b.logo }}</div>
          <div class="brand-info">
            <h3>{{ b.name }}</h3>
            <p>{{ b.description }}</p>
            <span class="count-badge">{{ b.productCount }} Products</span>
            <div class="actions">
              <a [routerLink]="['/brands', b.id]" class="view-details-btn">View Brand Profile</a>
              <a [routerLink]="['/products/brand', b.id]" class="primary button-link">Shop Products</a>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .brand-list-container {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .section-header {
      text-align: center;
      margin-bottom: 1rem;
    }
    .section-header h2 {
      font-size: 2rem;
      font-weight: 800;
      color: var(--ink);
    }
    .section-header p {
      color: var(--muted);
    }
    .brands-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
    }
    .brand-card {
      display: flex;
      gap: 1.25rem;
      padding: 1.5rem;
      transition: transform 0.2s, border-color 0.2s;
    }
    .brand-card:hover {
      transform: translateY(-4px);
      border-color: var(--accent);
    }
    .brand-logo-avatar {
      font-size: 2.2rem;
      background: var(--soft);
      width: 60px;
      height: 60px;
      border-radius: 12px;
      display: grid;
      place-items: center;
      flex-shrink: 0;
    }
    .brand-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex-grow: 1;
    }
    .brand-info h3 {
      font-size: 1.1rem;
      font-weight: 750;
      color: var(--ink);
    }
    .brand-info p {
      font-size: 0.85rem;
      color: var(--muted);
      margin-bottom: 0.5rem;
    }
    .count-badge {
      font-size: 0.72rem;
      font-weight: 800;
      background: var(--soft);
      color: var(--accent);
      padding: 2px 8px;
      border-radius: 4px;
      width: fit-content;
      margin-bottom: 0.75rem;
    }
    .actions {
      display: flex;
      gap: 10px;
      margin-top: auto;
    }
    .button-link {
      padding: 6px 12px;
      border-radius: 6px;
      font-size: 0.8rem;
      text-decoration: none;
      font-weight: 700;
    }
    .view-details-btn {
      padding: 6px 12px;
      font-size: 0.8rem;
      color: var(--muted);
      text-decoration: none;
      border: 1px solid var(--line);
      border-radius: 6px;
      font-weight: 600;
      display: inline-grid;
      place-items: center;
    }
    .view-details-btn:hover {
      background: var(--soft);
      color: var(--ink);
    }
  `]
})
export class BrandListComponent implements OnInit {
  private readonly brandService = inject(BrandService);

  readonly brands = signal<Brand[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.loadBrands();
  }

  loadBrands(): void {
    this.loading.set(true);
    this.error.set(false);

    this.brandService.getBrands().subscribe({
      next: (list) => {
        this.brands.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
      }
    });
  }

  trackById(index: number, brand: Brand): string {
    return brand.id;
  }
}
