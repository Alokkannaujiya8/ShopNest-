import { Injectable, signal, computed } from '@angular/core';
import { WishlistItem } from './api.models';

@Injectable({
  providedIn: 'root'
})
export class WishlistStateService {
  private readonly itemsSignal = signal<WishlistItem[]>([]);
  
  readonly items = this.itemsSignal.asReadonly();
  readonly count = computed(() => this.itemsSignal().length);

  setItems(items: WishlistItem[]) {
    this.itemsSignal.set(items);
  }

  addItem(item: WishlistItem) {
    this.itemsSignal.update(current => {
      if (current.some(x => x.productId === item.productId)) return current;
      return [...current, item];
    });
  }

  removeItem(productId: string) {
    this.itemsSignal.update(current => current.filter(x => x.productId !== productId));
  }

  clear() {
    this.itemsSignal.set([]);
  }

  hasItem(productId: string): boolean {
    return this.itemsSignal().some(x => x.productId === productId);
  }
}
