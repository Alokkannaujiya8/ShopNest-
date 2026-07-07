import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { BrandService, BrandDetails } from '../../core/brand.service';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Product } from '../../core/api.models';

@Component({
  selector: 'app-brand-details',
  standalone: false,
  template: `
    <section class="brand-details-container" *ngIf="details() as data">
      <!-- Brand Profile Card -->
      <div class="panel brand-profile-card">
        <div class="brand-profile-left">
          <div class="avatar-logo">{{ data.brand.logo }}</div>
          <div class="profile-info">
            <span class="eyebrow">Partner Manufacturer Profile</span>
            <h1>{{ data.brand.name }}</h1>
            <p>{{ data.brand.description }}</p>
          </div>
        </div>
        <div class="brand-stats">
          <span class="stat-value">{{ data.brand.productCount }}</span>
          <span class="stat-label">Products Active</span>
        </div>
      </div>

      <!-- Products Grid -->
      <div class="brand-products-area">
        <div class="panel-header">
          <h2>Products manufactured by {{ data.brand.name }}</h2>
          <a [routerLink]="['/products/brand', data.brand.id]" class="view-all-link">View in Catalog</a>
        </div>

        <div class="catalog-grid" *ngIf="data.products.length > 0; else noProducts">
          <!-- Product Card -->
          <div *ngFor="let p of data.products" class="product-card" [routerLink]="['/catalog/product', p.slug]">
            <div class="product-image-wrapper">
              <img 
                [src]="p.images[0]?.url || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=350&q=80'" 
                [alt]="p.name" 
              />
              <div class="badge discount" *ngIf="p.discountValue > 0">
                -{{ p.discountValue }}{{ p.discountType === 'Percentage' ? '%' : ' OFF' }}
              </div>
            </div>
            <div class="product-details">
              <span class="product-brand" *ngIf="p.brandName">{{ p.brandName }}</span>
              <h3 class="product-title">{{ p.name }}</h3>
              <div class="product-rating">
                <span class="stars">★ {{ p.averageRating | number:'1.1-1' }}</span>
                <span class="count">({{ p.reviewsCount }})</span>
              </div>
              <div class="product-price-row">
                <span class="current-price">{{ p.price | currency }}</span>
                <button class="cart-add-btn" type="button" (click)="addToCart(p, $event)">＋</button>
              </div>
            </div>
          </div>
        </div>
        <ng-template #noProducts>
          <div class="empty-state">No products found for this manufacturer.</div>
        </ng-template>
      </div>
    </section>

    <!-- Loading spinner -->
    <div class="loading-state" *ngIf="loading()">
      <div class="skeleton-banner"></div>
      <div class="skeleton-grid"></div>
    </div>

    <!-- Error state -->
    <div class="error-state" *ngIf="!loading() && error()">
      <p>Failed to load brand profile details.</p>
      <button class="primary" (click)="loadDetails()">Retry</button>
    </div>
  `,
  styles: [`
    .brand-details-container {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .brand-profile-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 2.5rem;
      background: linear-gradient(135deg, var(--accent) 0%, rgba(15, 118, 110, 0.8) 100%);
      color: #ffffff;
    }
    .brand-profile-left {
      display: flex;
      gap: 1.5rem;
      align-items: center;
    }
    .avatar-logo {
      width: 80px;
      height: 80px;
      background: rgba(255,255,255,0.2);
      border: 2px solid #ffffff;
      border-radius: 12px;
      display: grid;
      place-items: center;
      font-size: 2.5rem;
    }
    .profile-info .eyebrow {
      font-size: 0.72rem;
      text-transform: uppercase;
      font-weight: 700;
      letter-spacing: 0.1em;
      opacity: 0.9;
    }
    .profile-info h1 {
      font-size: 2.2rem;
      font-weight: 850;
      margin-top: 0.2rem;
      margin-bottom: 0.4rem;
    }
    .profile-info p {
      font-size: 0.95rem;
      opacity: 0.95;
    }
    .brand-stats {
      display: flex;
      flex-direction: column;
      align-items: center;
      background: rgba(255,255,255,0.15);
      padding: 1rem 1.5rem;
      border-radius: 8px;
      min-width: 140px;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: 900;
    }
    .stat-label {
      font-size: 0.75rem;
      text-transform: uppercase;
      font-weight: 700;
      opacity: 0.9;
    }
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.25rem;
    }
    .panel-header h2 {
      font-size: 1.3rem;
      font-weight: 800;
      color: var(--ink);
    }
    .view-all-link {
      font-size: 0.85rem;
      color: var(--accent);
      text-decoration: none;
      font-weight: 700;
    }
    .view-all-link:hover {
      text-decoration: underline;
    }
    .catalog-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: 1.5rem;
    }
    .product-card {
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 12px;
      overflow: hidden;
      cursor: pointer;
      transition: transform 0.2s;
    }
    .product-card:hover {
      transform: translateY(-4px);
    }
    .product-image-wrapper {
      position: relative;
      aspect-ratio: 1.15;
      overflow: hidden;
    }
    .product-image-wrapper img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    .product-details {
      padding: 1rem;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .product-brand {
      font-size: 0.7rem;
      text-transform: uppercase;
      color: var(--muted);
      font-weight: 700;
    }
    .product-title {
      font-size: 0.95rem;
      font-weight: 700;
      color: var(--ink);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .product-rating {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75rem;
    }
    .stars {
      color: #fbbf24;
    }
    .count {
      color: var(--muted);
    }
    .product-price-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.5rem;
    }
    .current-price {
      font-size: 1.05rem;
      font-weight: 800;
      color: var(--accent);
    }
    .cart-add-btn {
      background: var(--accent);
      color: #ffffff;
      border: 0;
      width: 32px;
      height: 32px;
      border-radius: 6px;
      font-size: 1rem;
      cursor: pointer;
    }
    .badge.discount {
      position: absolute;
      top: 10px;
      left: 10px;
      background: var(--accent);
      color: #ffffff;
      padding: 3px 6px;
      border-radius: 4px;
      font-size: 0.7rem;
      font-weight: 800;
    }
    .empty-state {
      padding: 3rem;
      text-align: center;
      color: var(--muted);
    }
    @media (max-width: 768px) {
      .brand-profile-card {
        flex-direction: column;
        align-items: flex-start;
        gap: 1.5rem;
        padding: 2rem 1.5rem;
      }
      .brand-stats {
        width: 100%;
        align-items: center;
      }
    }
  `]
})
export class BrandDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly brandService = inject(BrandService);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly details = signal<BrandDetails | null>(null);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.loadDetails(id);
      }
    });
  }

  loadDetails(id?: string): void {
    const targetId = id || this.route.snapshot.paramMap.get('id');
    if (!targetId) return;

    this.loading.set(true);
    this.error.set(false);

    this.brandService.getBrandDetails(targetId).subscribe({
      next: (res) => {
        this.details.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
      }
    });
  }

  addToCart(p: Product, event: MouseEvent): void {
    event.stopPropagation();
    event.preventDefault();

    if (!this.api.currentUser()) {
      this.toast.warning('Please log in to add items to your cart.');
      return;
    }

    this.api.addToCart(p.id, 1).subscribe({
      next: () => {
        this.toast.success(`"${p.name}" added to cart!`);
        this.api.refreshCartCount();
      },
      error: () => this.toast.error('Failed to add item to cart.')
    });
  }
}
