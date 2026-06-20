import { Component, OnInit } from '@angular/core';
import { Category, DashboardStats, Order, OrderStatus, Product } from '../../core/api.models';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';

@Component({
  selector: 'app-admin',
  standalone: false,
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss',
})
export class AdminComponent implements OnInit {
  stats?: DashboardStats;
  products: Product[] = [];
  categories: Category[] = [];
  orders: Order[] = [];
  categoryName = '';
  editingId = '';
  productForm = {
    name: '',
    description: '',
    price: 0,
    stockQuantity: 0,
    categoryId: '',
    isActive: true,
  };
  notice = '';

  constructor(
    private readonly api: ApiService,
    readonly signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.api.dashboard().subscribe((stats) => (this.stats = stats));
    this.api.categories().subscribe((categories) => {
      this.categories = categories;
      this.productForm.categoryId ||= categories[0]?.id ?? '';
    });
    this.api.products({ page: 1, pageSize: 50 }).subscribe((result) => (this.products = result.items));
    this.api.allOrders().subscribe((result) => (this.orders = result.items));
  }

  saveCategory(): void {
    this.api.createCategory(this.categoryName).subscribe((category) => {
      this.categories = [...this.categories, category];
      this.categoryName = '';
    });
  }

  edit(product: Product): void {
    this.editingId = product.id;
    this.productForm = {
      name: product.name,
      description: product.description,
      price: product.price,
      stockQuantity: product.stockQuantity,
      categoryId: product.category.id,
      isActive: product.isActive,
    };
  }

  resetForm(): void {
    this.editingId = '';
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      stockQuantity: 0,
      categoryId: this.categories[0]?.id ?? '',
      isActive: true,
    };
  }

  saveProduct(): void {
    const request = this.editingId
      ? this.api.updateProduct(this.editingId, this.productForm)
      : this.api.createProduct(this.productForm);

    request.subscribe(() => {
      this.notice = 'Product saved.';
      this.resetForm();
      this.reload();
    });
  }

  updateInventory(product: Product): void {
    this.api.updateInventory(product.id, product.stockQuantity, product.isActive).subscribe(() => {
      this.notice = `${product.name} inventory updated.`;
    });
  }

  upload(product: Product, event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.api.uploadProductImage(product.id, file, true).subscribe(() => {
      this.notice = `${product.name} image uploaded.`;
      this.reload();
    });
  }

  deleteProduct(product: Product): void {
    this.api.deleteProduct(product.id).subscribe(() => {
      this.notice = `${product.name} deleted.`;
      this.reload();
    });
  }

  updateOrder(order: Order, status: OrderStatus): void {
    this.api.updateOrderStatus(order.id, status).subscribe(() => this.reload());
  }
}
