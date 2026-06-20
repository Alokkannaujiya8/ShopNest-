import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { Category, Product } from '../../core/api.models';

@Component({
  selector: 'app-catalog',
  standalone: false,
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  query = '';
  categoryId = '';
  minPrice?: number;
  maxPrice?: number;
  inStock = false;
  page = 1;
  totalPages = 1;
  loading = false;
  notice = '';

  constructor(readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.categories().subscribe((categories) => (this.categories = categories));
    this.load();
  }

  load(page = 1): void {
    this.page = page;
    this.loading = true;
    this.api
      .products({
        query: this.query,
        categoryId: this.categoryId,
        minPrice: this.minPrice,
        maxPrice: this.maxPrice,
        inStock: this.inStock || null,
        page: this.page,
        pageSize: 12,
      })
      .subscribe({
        next: (result) => {
          this.products = result.items;
          this.totalPages = result.totalPages || 1;
          this.loading = false;
        },
        error: () => (this.loading = false),
      });
  }

  add(product: Product): void {
    if (!this.api.currentUser()) {
      this.notice = 'Login required before adding items to cart.';
      return;
    }

    this.api.addToCart(product.id, 1).subscribe(() => {
      this.notice = `${product.name} added to cart.`;
    });
  }

  image(product: Product): string {
    return product.images.find((item) => item.isPrimary)?.url || product.images[0]?.url || 'https://images.unsplash.com/photo-1557821552-17105176677c?auto=format&fit=crop&w=900&q=80';
  }
}
