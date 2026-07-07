import { Injectable, inject } from '@angular/core';
import { Observable, map, switchMap } from 'rxjs';
import { ApiService } from './api.service';
import { Product } from './api.models';

export interface Brand {
  id: string;
  name: string;
  logo: string;
  description: string;
  productCount: number;
}

export interface BrandDetails {
  brand: Brand;
  products: Product[];
}

@Injectable({
  providedIn: 'root'
})
export class BrandService {
  private readonly api = inject(ApiService);

  getBrands(): Observable<Brand[]> {
    // Dynamically retrieve unique brands from products catalog
    return this.api.products({ page: 1, pageSize: 250 }).pipe(
      map(res => {
        const brandsMap = new Map<string, Brand>();
        
        res.items.forEach(p => {
          if (p.brandId && p.brandName) {
            if (brandsMap.has(p.brandId)) {
              const b = brandsMap.get(p.brandId)!;
              b.productCount++;
            } else {
              brandsMap.set(p.brandId, {
                id: p.brandId,
                name: p.brandName,
                logo: this.getLogoForBrand(p.brandName),
                description: `Premium designer products manufactured by ${p.brandName}.`,
                productCount: 1
              });
            }
          }
        });
        
        return Array.from(brandsMap.values());
      })
    );
  }

  getBrandDetails(brandId: string): Observable<BrandDetails> {
    return this.getBrands().pipe(
      switchMap((brands) => {
        const brand = brands.find(b => b.id === brandId) || {
          id: brandId,
          name: 'Premium Manufacturer',
          logo: '🏷️',
          description: 'High-quality designer products and essentials.',
          productCount: 0
        };

        return this.api.products({ brandId, page: 1, pageSize: 20 }).pipe(
          map(productsRes => ({
            brand,
            products: productsRes.items
          }))
        );
      })
    );
  }

  private getLogoForBrand(brandName: string): string {
    const name = brandName.toLowerCase();
    if (name.includes('apple')) return '🍎';
    if (name.includes('tech') || name.includes('aero')) return '⚡';
    if (name.includes('eco') || name.includes('green')) return '🌿';
    if (name.includes('luxury') || name.includes('gold')) return '✨';
    return '🏷️';
  }
}
