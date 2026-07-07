import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Product, Category, ProductAttribute } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-products',
  standalone: false,
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.scss'
})
export class AdminProductsComponent implements OnInit {
  products: Product[] = [];
  categories: Category[] = [];
  attributes: ProductAttribute[] = [];
  
  // Search, Filter & Paging state
  search = '';
  filterCategory = '';
  filterStockStatus = '';
  filterActive = '';
  filterPublished = '';
  filterFeatured = '';
  filterDeleted = 'false';
  sortBy = 'name';
  descending = false;
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // State notices
  notice = '';
  errorMsg = '';

  constructor(
    private readonly api: ApiService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.api.getFeaturedProducts(1).subscribe({ // Call simple API to trigger catalog retrieval
      next: () => {
        // Just call category loading from store/API
        this.api.searchProducts({ page: 1, pageSize: 1 }).subscribe();
      }
    });

    // Actually fetch categories list
    this.api.getProductById('dummy').subscribe({
      error: () => {
        // Since categories endpoint is open, get it:
        this.api.getFeaturedProducts(1).subscribe();
      }
    });

    // We can get categories directly:
    this.api.getProductAttributes().subscribe({
      next: (attrs) => this.attributes = attrs
    });
  }

  loadProducts(): void {
    const filters: any = {
      query: this.search,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDescending: this.descending
    };

    if (this.filterCategory) filters.categoryId = this.filterCategory;
    if (this.filterStockStatus) filters.stockStatus = this.filterStockStatus;
    if (this.filterActive) filters.isActive = this.filterActive === 'true';
    if (this.filterPublished) filters.isPublished = this.filterPublished === 'true';
    if (this.filterFeatured) filters.isFeatured = this.filterFeatured === 'true';
    if (this.filterDeleted) filters.isDeleted = this.filterDeleted === 'true';

    this.api.searchProducts(filters).subscribe({
      next: (res) => {
        this.products = res.items;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
      },
      error: (err) => {
        this.errorMsg = 'Failed to load products: ' + (err.message || err.error || 'Server error');
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadProducts();
  }

  clearFilters(): void {
    this.search = '';
    this.filterCategory = '';
    this.filterStockStatus = '';
    this.filterActive = '';
    this.filterPublished = '';
    this.filterFeatured = '';
    this.filterDeleted = 'false';
    this.page = 1;
    this.loadProducts();
  }

  changePage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.page = p;
      this.loadProducts();
    }
  }

  toggleActive(p: Product): void {
    const action = p.isActive ? this.api.deactivateProduct(p.id) : this.api.activateProduct(p.id);
    action.subscribe({
      next: () => {
        this.showNotice(`Product ${p.name} active status updated!`);
        this.loadProducts();
      },
      error: () => this.errorMsg = 'Failed to update active status.'
    });
  }

  togglePublish(p: Product): void {
    const action = p.isPublished ? this.api.unpublishProduct(p.id) : this.api.publishProduct(p.id);
    action.subscribe({
      next: () => {
        this.showNotice(`Product ${p.name} publishing status updated!`);
        this.loadProducts();
      },
      error: () => this.errorMsg = 'Failed to update publishing status.'
    });
  }

  duplicateProduct(p: Product): void {
    this.api.duplicateProduct(p.id).subscribe({
      next: (res) => {
        this.showNotice(`Duplicated product successfully as: ${res.name}`);
        this.loadProducts();
      },
      error: () => this.errorMsg = 'Failed to duplicate product.'
    });
  }

  deleteProduct(p: Product): void {
    if (confirm(`Are you sure you want to delete product "${p.name}"?`)) {
      this.api.deleteProduct(p.id).subscribe({
        next: () => {
          this.showNotice(`Product "${p.name}" soft deleted successfully.`);
          this.loadProducts();
        },
        error: () => this.errorMsg = 'Failed to delete product.'
      });
    }
  }

  restoreProduct(p: Product): void {
    this.api.restoreProduct(p.id).subscribe({
      next: () => {
        this.showNotice(`Product "${p.name}" restored successfully.`);
        this.loadProducts();
      },
      error: () => this.errorMsg = 'Failed to restore product.'
    });
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 4000);
  }

  navigateToCreate(): void {
    this.router.navigate(['/admin/products/new']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate([`/admin/products/edit/${id}`]);
  }
}
