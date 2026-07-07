import { Component, OnInit, inject, signal } from '@angular/core';
import { CategoryService } from '../../core/category.service';
import { Category } from '../../core/api.models';

@Component({
  selector: 'app-category-list',
  standalone: false,
  template: `
    <section class="category-list-container">
      <div class="section-header">
        <h2>Product Categories</h2>
        <p>Browse products by their curated categories</p>
      </div>

      <!-- Loading skeleton -->
      <div class="loading-state" *ngIf="loading()">
        <div class="categories-grid">
          <div class="skeleton-card" *ngFor="let item of [1, 2, 3, 4]"></div>
        </div>
      </div>

      <!-- Error / Empty state -->
      <div class="error-state" *ngIf="!loading() && error()">
        <p>Failed to retrieve product categories.</p>
        <button class="primary" (click)="loadCategories()">Retry</button>
      </div>

      <div class="empty-state" *ngIf="!loading() && !error() && categories().length === 0">
        No product categories found.
      </div>

      <!-- Categories grid list -->
      <div class="categories-grid" *ngIf="!loading() && !error() && categories().length > 0">
        <div *ngFor="let cat of categories(); trackBy: trackById" class="panel category-card">
          <div class="category-avatar">📁</div>
          <div class="category-info">
            <h3>{{ cat.name }}</h3>
            <p>Curated product designs catalog.</p>
            <div class="actions">
              <a [routerLink]="['/categories', cat.id]" class="view-details-btn">View Hierarchy</a>
              <a [routerLink]="['/products/category', cat.id]" class="primary button-link">Shop Products</a>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .category-list-container {
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
    .categories-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 1.5rem;
    }
    .category-card {
      display: flex;
      gap: 1.25rem;
      padding: 1.5rem;
      transition: transform 0.2s, border-color 0.2s;
    }
    .category-card:hover {
      transform: translateY(-4px);
      border-color: var(--accent);
    }
    .category-avatar {
      font-size: 2.2rem;
      background: var(--soft);
      width: 60px;
      height: 60px;
      border-radius: 12px;
      display: grid;
      place-items: center;
      flex-shrink: 0;
    }
    .category-info {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex-grow: 1;
    }
    .category-info h3 {
      font-size: 1.1rem;
      font-weight: 750;
      color: var(--ink);
    }
    .category-info p {
      font-size: 0.9rem;
      color: var(--muted);
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
export class CategoryListComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);

  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.error.set(false);

    this.categoryService.getCategories().subscribe({
      next: (list) => {
        this.categories.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set(true);
      }
    });
  }

  trackById(index: number, cat: Category): string {
    return cat.id;
  }
}
