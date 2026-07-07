import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { WishlistService } from '../../core/wishlist.service';
import { WishlistItem, Category } from '../../core/api.models';

@Component({
  selector: 'app-wishlist',
  standalone: false,
  templateUrl: './wishlist.component.html',
  styleUrl: './wishlist.component.scss'
})
export class WishlistComponent implements OnInit {
  wishlistItems: WishlistItem[] = [];
  categories: Category[] = [];
  availableBrands: { id: string; name: string }[] = [];

  // Search & Filtering
  search = '';
  filterCategory = '';
  filterBrand = '';
  filterStockStatus = '';
  filterDiscounted = false;

  // Sorting
  sortBy = 'recentlyadded';
  sortDescending = true;

  // Pagination
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 1;

  loading = false;
  notice = '';
  errorMsg = '';

  constructor(
    private readonly api: ApiService,
    private readonly wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.loadFiltersData();
    this.load();
  }

  loadFiltersData(): void {
    // Load categories
    this.api.categories().subscribe({
      next: (res) => this.categories = res
    });

    // Load first page of wishlist to dynamically build brand filters
    this.api.getWishlistItems({ page: 1, pageSize: 50 }).subscribe({
      next: (res) => {
        const brandsMap = new Map<string, string>();
        res.items.forEach(x => {
          if (x.brandName) {
            // Since API returns BrandName, we can filter by name or map it
            brandsMap.set(x.brandName, x.brandName);
          }
        });
        this.availableBrands = Array.from(brandsMap.entries()).map(([name]) => ({ id: name, name }));
      }
    });
  }

  load(page = 1): void {
    this.page = page;
    this.loading = true;
    this.errorMsg = '';

    const filters: any = {
      page: this.page,
      pageSize: this.pageSize,
      query: this.search || null,
      categoryId: this.filterCategory || null,
      stockStatus: this.filterStockStatus || null,
      isDiscounted: this.filterDiscounted || null,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending
    };

    // If filter brand is specified, we pass it dynamically
    if (this.filterBrand) {
      filters.query = this.filterBrand; // Brand name search matches search query
    }

    this.api.getWishlistItems(filters).subscribe({
      next: (res) => {
        this.wishlistItems = res.items;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = 'Failed to load wishlist: ' + (err.error || err.message);
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.search = '';
    this.filterCategory = '';
    this.filterBrand = '';
    this.filterStockStatus = '';
    this.filterDiscounted = false;
    this.sortBy = 'recentlyadded';
    this.sortDescending = true;
    this.page = 1;
    this.load();
  }

  removeFromWishlist(item: WishlistItem): void {
    this.wishlistService.removeFromWishlist(item.productId).subscribe({
      next: () => {
        this.showNotice(`${item.productName} removed from wishlist.`);
        this.load(this.page);
      },
      error: (err) => this.errorMsg = 'Failed to remove item: ' + (err.error || err.message)
    });
  }

  clearWishlist(): void {
    if (!confirm('Are you sure you want to empty your wishlist?')) return;

    this.wishlistService.clearWishlist().subscribe({
      next: () => {
        this.showNotice('Wishlist cleared successfully.');
        this.load();
      },
      error: (err) => this.errorMsg = 'Failed to clear wishlist: ' + (err.error || err.message)
    });
  }

  moveToCart(item: WishlistItem): void {
    if (item.stockQuantity === 0) {
      this.errorMsg = 'This product is currently out of stock.';
      return;
    }

    this.api.moveWishlistItemToCart(item.productId).subscribe({
      next: () => {
        this.showNotice(`${item.productName} moved to cart.`);
        this.api.refreshCartCount();
        this.load(this.page);
      },
      error: (err) => this.errorMsg = 'Failed to move item: ' + (err.error || err.message)
    });
  }

  changePage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.load(p);
    }
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }

  image(item: WishlistItem): string {
    return item.imageUrl || 'https://images.unsplash.com/photo-1557821552-17105176677c?auto=format&fit=crop&w=900&q=80';
  }

  onSortChange(): void {
    this.sortDescending = this.sortBy === 'recentlyadded' || this.sortBy === 'price_desc';
    
    // Normalize SortBy values
    if (this.sortBy === 'price_desc' || this.sortBy === 'price_asc') {
      this.sortBy = 'price';
    }
    
    this.page = 1;
    this.load();
  }
}
