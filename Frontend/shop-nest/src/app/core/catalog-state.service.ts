import { Injectable, signal } from '@angular/core';

export interface CatalogState {
  query: string;
  categoryId: string;
  brandId: string;
  minPrice?: number;
  maxPrice?: number;
  minRating?: number;
  sortBy: string;
  sortDescending: boolean;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class CatalogStateService {
  private readonly state = signal<CatalogState>({
    query: '',
    categoryId: '',
    brandId: '',
    sortBy: 'relevance',
    sortDescending: false,
    page: 1,
    pageSize: 12
  });

  getState() {
    return this.state.asReadonly();
  }

  updateState(updates: Partial<CatalogState>) {
    this.state.update(current => ({
      ...current,
      ...updates
    }));
  }

  reset() {
    this.state.set({
      query: '',
      categoryId: '',
      brandId: '',
      sortBy: 'relevance',
      sortDescending: false,
      page: 1,
      pageSize: 12
    });
  }
}
