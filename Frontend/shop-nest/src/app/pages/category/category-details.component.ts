import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoryService, CategoryDetails } from '../../core/category.service';
import { ApiService } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Product } from '../../core/api.models';

@Component({
  selector: 'app-category-details',
  standalone: false,
  template: `
    <section class="category-details-container" *ngIf="details() as data">
      <!-- Category Banner Card -->
      <div class="panel category-banner-card">
        <div class="banner-overlay"></div>
        <div class="banner-content">
          <p class="parent-trail" *ngIf="data.parentCategory">
            <a [routerLink]="['/categories', data.parentCategory.id]">{{ data.parentCategory.name }}</a>
            <span class="sep">/</span>
          </p>
          <h1>{{ data.category.name }}</h1>
          <p class="desc">Explore premium products matching {{ data.category.name }}.</p>
        </div>
      </div>

      <div class="category-layout-grid">
        <!-- Sidebar Navigation for Child Categories -->
        <aside class="panel hierarchy-sidebar" *ngIf="data.childCategories.length > 0">
          <h2>Subcategories</h2>
          <nav class="subcategories-nav">
            <a 
              *ngFor="let child of data.childCategories" 
              [routerLink]="['/categories', child.id]"
              class="subcategory-link"
            >
              <span class="icon">📁</span>
              <span>{{ child.name }}</span>
            </a>
          </nav>
        </aside>

        <!-- Category Products -->
        <div class="category-products-area">
          <div class="panel-header">
            <h2>Products in {{ data.category.name }}</h2>
            <a [routerLink]="['/products/category', data.category.id]" class="view-all-link">View in Catalog</a>
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
            <div class="empty-state">No products found in this category.</div>
          </ng-template>
        </div>
      </div>
    </section>

    <!-- Loading spinner -->
    <div class="loading-state" *ngIf="loading()">
      <div class="skeleton-banner"></div>
      <div class="skeleton-grid"></div>
    </div>

    <!-- Error state -->
    <div class="error-state" *ngIf="!loading() && error()">
      <p>Failed to load category details.</p>
      <button class="primary" (click)="loadDetails()">Retry</button>
    </div>
  `,
  styles: [`
    .category-details-container {
      display: flex;
      flex-direction: column;
      gap: 2rem;
    }
    .category-banner-card {
      position: relative;
      height: 220px;
      background: linear-gradient(135deg, var(--accent) 0%, rgba(15, 118, 110, 0.7) 100%), 
                  url('https://images.unsplash.com/photo-1441986300917-64674bd600d8?auto=format&fit=crop&w=1200&q=80');
      background-size: cover;
      background-position: center;
      display: flex;
      align-items: center;
      padding: 0 3rem;
      color: #ffffff;
      overflow: hidden;
    }
    .banner-overlay {
      position: absolute;
      inset: 0;
      background: rgba(0,0,0,0.15);
      z-index: 1;
    }
    .banner-content {
      position: relative;
      z-index: 2;
    }
    .banner-content h1 {
      font-size: 2.5rem;
      font-weight: 850;
      margin-bottom: 0.5rem;
    }
    .banner-content .desc {
      font-size: 1.05rem;
      opacity: 0.9;
    }
    .parent-trail {
      font-size: 0.8rem;
      text-transform: uppercase;
      font-weight: 700;
      margin-bottom: 0.25rem;
      opacity: 0.9;
    }
    .parent-trail a {
      color: #ffffff;
      text-decoration: none;
    }
    .parent-trail a:hover {
      text-decoration: underline;
    }
    .sep {
      margin: 0 6px;
    }
    .category-layout-grid {
      display: grid;
      grid-template-columns: 280px 1fr;
      gap: 2rem;
    }
    .hierarchy-sidebar h2 {
      font-size: 1.1rem;
      font-weight: 750;
      margin-bottom: 1rem;
      color: var(--ink);
    }
    .subcategories-nav {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .subcategory-link {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px 12px;
      border-radius: 6px;
      text-decoration: none;
      color: var(--muted);
      font-weight: 600;
      font-size: 0.9rem;
      transition: background-color 0.2s, color 0.2s;
    }
    .subcategory-link:hover {
      background: var(--soft);
      color: var(--ink);
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
    @media (max-width: 1024px) {
      .category-layout-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class CategoryDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly categoryService = inject(CategoryService);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly details = signal<CategoryDetails | null>(null);
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

    this.categoryService.getCategoryDetails(targetId).subscribe({
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
