import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, map } from 'rxjs';
import { ApiService } from './api.service';
import { Category, Product } from './api.models';

export interface HomeData {
  categories: Category[];
  featuredProducts: Product[];
  newArrivals: Product[];
  bestSellers: Product[];
}

@Injectable({
  providedIn: 'root'
})
export class HomeService {
  private readonly api = inject(ApiService);

  getHomeData(): Observable<HomeData> {
    return forkJoin({
      categories: this.api.categories(),
      featuredProducts: this.api.getFeaturedProducts(8),
      newArrivals: this.api.getNewArrivalProducts(8),
      bestSellers: this.api.getBestSellerProducts(8)
    });
  }
}
