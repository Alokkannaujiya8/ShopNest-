import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, map, switchMap } from 'rxjs';
import { ApiService } from './api.service';
import { Category, Product, AdminCategoryDto } from './api.models';

export interface CategoryDetails {
  category: AdminCategoryDto;
  parentCategory?: Category | null;
  childCategories: Category[];
  products: Product[];
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly api = inject(ApiService);

  getCategories(): Observable<Category[]> {
    return this.api.categories();
  }

  getCategoryDetails(categoryId: string): Observable<CategoryDetails> {
    return this.api.getCategoryById(categoryId).pipe(
      switchMap((categoryRes) => {
        const cat = categoryRes.data;
        if (!cat) throw new Error('Category not found');

        return forkJoin({
          productsRes: this.api.products({ categoryId, page: 1, pageSize: 20 }),
          categoriesList: this.api.categories()
        }).pipe(
          map(({ productsRes, categoriesList }) => {
            // Find parent if parentId is specified
            const parentCategory = cat.parentId 
              ? categoriesList.find(c => c.id === cat.parentId) 
              : null;

            // Find child categories (categories whose parentId matches this category's id)
            // Note: AdminCategoryDto or Category might have children. Let's filter from list.
            const childCategories = categoriesList.filter(c => (c as any).parentId === cat.id);

            return {
              category: cat,
              parentCategory,
              childCategories,
              products: productsRes.items
            };
          })
        );
      })
    );
  }
}
