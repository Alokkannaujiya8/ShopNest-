import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { WishlistService } from '../../core/wishlist.service';
import { Category, Product, Warehouse } from '../../core/api.models';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { SeoService } from '../../core/seo.service';
import { CatalogStateService } from '../../core/catalog-state.service';

@Component({
  selector: 'app-catalog',
  standalone: false,
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit, OnDestroy {
  // Products listing
  products: Product[] = [];
  categories: Category[] = [];
  warehouses: Warehouse[] = [];

  // View state
  viewMode: 'grid' | 'list' = 'grid';
  loading = false;
  notice = '';
  errorMsg = '';

  // Advanced Filters Form
  query = '';
  categoryId = '';
  brandId = '';
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  selectedColor = '';
  selectedSize = '';
  selectedMaterial = '';
  stockStatus = ''; // 'InStock', 'OutOfStock'
  isFeatured = false;
  isNewArrival = false;
  isBestSeller = false;

  // Sorting
  sortBy = 'relevance';
  sortDescending = false;

  // Pagination
  page = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 1;

  // Search Autocomplete / Suggestions
  suggestions: Product[] = [];
  showSuggestions = false;
  searchHistory: string[] = [];
  popularSearches: string[] = [];
  private readonly searchSubject = new Subject<string>();
  private searchSub?: Subscription;

  // Common attribute filters options (Seed)
  availableColors = ['Black', 'White', 'Red', 'Blue', 'Green', 'Gray', 'Yellow'];
  availableSizes = ['XS', 'S', 'M', 'L', 'XL', 'XXL', '38', '40', '42', '44'];
  availableMaterials = ['Cotton', 'Polyester', 'Wool', 'Leather', 'Metal', 'Plastic', 'Glass'];
  availableBrands: { id: string; name: string }[] = [];

  constructor(
    readonly api: ApiService, 
    private readonly seo: SeoService,
    private readonly route: ActivatedRoute,
    private readonly stateService: CatalogStateService,
    private readonly wishlistService: WishlistService
  ) {}

  ngOnInit(): void {
    this.seo.setMetaTags(
      'Catalog & Shop',
      'Discover and shop thousands of premium products across our electronics, home, fashion, and lifestyle catalogs.'
    );

    // Restore state from CatalogStateService
    const saved = this.stateService.getState()();
    this.query = saved.query;
    this.categoryId = saved.categoryId;
    this.brandId = saved.brandId;
    this.sortBy = saved.sortBy;
    this.sortDescending = saved.sortDescending;
    this.page = saved.page;
    this.pageSize = saved.pageSize;

    // Initial data loaders
    this.loadFiltersData();
    this.loadSearchSuggestionsDebounce();
    this.loadSearchHistoryAndPopular();

    // Listen to query parameters (for /search and /search/results)
    this.route.queryParamMap.subscribe(qparams => {
      const q = qparams.get('query');
      if (q !== null) {
        this.query = q;
        this.load();
      }
    });

    // Listen to route params for categoryId and brandId
    this.route.paramMap.subscribe(params => {
      const catId = params.get('categoryId');
      const bId = params.get('brandId');
      
      if (catId !== null) this.categoryId = catId;
      if (bId !== null) this.brandId = bId;
      
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  loadFiltersData(): void {
    // Load categories
    this.api.categories().subscribe({
      next: (res) => this.categories = res
    });

    // Extract brands from existing products catalog to build filters dynamically
    this.api.products({ page: 1, pageSize: 50 }).subscribe({
      next: (res) => {
        const brandsMap = new Map<string, string>();
        res.items.forEach(p => {
          if (p.brandId && p.brandName) {
            brandsMap.set(p.brandId, p.brandName);
          }
        });
        this.availableBrands = Array.from(brandsMap.entries()).map(([id, name]) => ({ id, name }));
      }
    });
  }

  loadSearchSuggestionsDebounce(): void {
    this.searchSub = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((term) => {
        if (!term || term.length < 2) {
          this.suggestions = [];
          return [];
        }
        return this.api.getSuggestions(term);
      })
    ).subscribe({
      next: (res) => {
        this.suggestions = res;
      }
    });
  }

  loadSearchHistoryAndPopular(): void {
    // Guest handles popular searches
    this.api.getPopularSearches(6).subscribe({
      next: (res) => this.popularSearches = res
    });

    // Logged in user gets personalized search history
    if (this.api.currentUser()) {
      this.api.getSearchHistory().subscribe({
        next: (res) => this.searchHistory = res
      });
    }
  }

  // Typeahead query triggers
  onQueryChange(): void {
    this.showSuggestions = this.query.trim().length > 1;
    this.searchSubject.next(this.query);
  }

  selectSuggestion(prod: Product): void {
    this.query = prod.name;
    this.showSuggestions = false;
    this.suggestions = [];
    this.onSearch();
  }

  selectSearchTerm(term: string): void {
    this.query = term;
    this.showSuggestions = false;
    this.onSearch();
  }

  hideSuggestions(): void {
    // Delay slightly to allow suggestion clicks to register
    setTimeout(() => this.showSuggestions = false, 250);
  }

  onSearch(): void {
    this.page = 1;
    this.load();
    this.loadSearchHistoryAndPopular();
  }

  load(page = 1): void {
    this.page = page;
    this.loading = true;
    this.errorMsg = '';

    this.stateService.updateState({
      query: this.query,
      categoryId: this.categoryId,
      brandId: this.brandId,
      minPrice: this.minPrice,
      maxPrice: this.maxPrice,
      minRating: this.minRating,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending,
      page: this.page,
      pageSize: this.pageSize
    });

    const filters: any = {
      page: this.page,
      pageSize: this.pageSize,
      query: this.query || null,
      categoryId: this.categoryId || null,
      brandId: this.brandId || null,
      minPrice: this.minPrice || null,
      maxPrice: this.maxPrice || null,
      minRating: this.minRating || null,
      color: this.selectedColor || null,
      size: this.selectedSize || null,
      material: this.selectedMaterial || null,
      stockStatus: this.stockStatus || null,
      isFeatured: this.isFeatured || null,
      isNewArrival: this.isNewArrival || null,
      isBestSeller: this.isBestSeller || null,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending
    };

    this.api.products(filters).subscribe({
      next: (result) => {
        this.products = result.items;
        this.totalPages = result.totalPages || 1;
        this.totalCount = result.totalCount || 0;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = 'Failed to load catalog: ' + (err.error || err.message);
      }
    });
  }

  // Quick Filters
  setCategory(catId: string): void {
    this.categoryId = catId;
    this.page = 1;
    this.load();
  }

  setBrand(brandId: string): void {
    this.brandId = brandId;
    this.page = 1;
    this.load();
  }

  setRating(rating: number): void {
    this.minRating = rating;
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.query = '';
    this.categoryId = '';
    this.brandId = '';
    this.minPrice = undefined;
    this.maxPrice = undefined;
    this.minRating = undefined;
    this.selectedColor = '';
    this.selectedSize = '';
    this.selectedMaterial = '';
    this.stockStatus = '';
    this.isFeatured = false;
    this.isNewArrival = false;
    this.isBestSeller = false;
    this.sortBy = 'relevance';
    this.sortDescending = false;
    this.page = 1;
    this.load();
  }

  // Cart actions
  add(product: Product): void {
    if (!this.api.currentUser()) {
      this.notice = 'Login required before adding items to cart.';
      setTimeout(() => this.notice = '', 3000);
      return;
    }

    this.api.addToCart(product.id, 1).subscribe({
      next: () => {
        this.notice = `${product.name} added to cart.`;
        this.api.refreshCartCount();
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => {
        this.errorMsg = 'Failed to add item: ' + (err.error || err.message);
        setTimeout(() => this.errorMsg = '', 3000);
      }
    });
  }

  addToWishlist(product: Product): void {
    if (!this.api.currentUser()) {
      this.notice = 'Login required before adding items to wishlist.';
      setTimeout(() => this.notice = '', 3000);
      return;
    }

    this.wishlistService.addToWishlist(product.id).subscribe({
      next: () => {
        this.notice = `${product.name} added to wishlist.`;
        setTimeout(() => this.notice = '', 3000);
      },
      error: (err) => {
        this.errorMsg = 'Failed to add to wishlist: ' + (err.error || err.message);
        setTimeout(() => this.errorMsg = '', 3000);
      }
    });
  }

  // Helpers
  image(product: Product): string {
    return product.images?.find((item) => item.isPrimary)?.url || product.images?.[0]?.url || 'https://images.unsplash.com/photo-1557821552-17105176677c?auto=format&fit=crop&w=900&q=80';
  }

  changePage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.load(p);
    }
  }

  onSortChange(): void {
    this.sortDescending = this.sortBy === 'price_desc' || this.sortBy === 'newest' || this.sortBy === 'highestrated' || this.sortBy === 'mostreviewed' || this.sortBy === 'za';
    
    // Normalize SortBy values passed to backend
    if (this.sortBy === 'price_desc' || this.sortBy === 'price_asc') {
      this.sortBy = 'price';
    }
    
    this.page = 1;
    this.load();
  }
}
