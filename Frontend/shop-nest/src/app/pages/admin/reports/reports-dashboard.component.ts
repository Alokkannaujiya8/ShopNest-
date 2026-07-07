import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../core/api.service';
import {
  DashboardSummary,
  SalesReport,
  RevenueReport,
  OrdersReport,
  CustomerReport,
  ProductReport,
  InventoryReport,
  PaymentReport,
  CouponReport,
  ReviewReport,
  Category
} from '../../../core/api.models';

@Component({
  selector: 'app-reports-dashboard',
  standalone: false,
  templateUrl: './reports-dashboard.component.html',
  styleUrl: './reports-dashboard.component.scss'
})
export class ReportsDashboardComponent implements OnInit {
  activeTab = 'dashboard';
  loading = false;
  exporting = false;
  errorMsg = '';
  notice = '';

  // Dropdown data
  categories: Category[] = [];
  brands: { id: string; name: string }[] = [];

  // Filter parameters
  startDate = this.getPastDate(30);
  endDate = this.getTodayDate();
  categoryId = '';
  brandId = '';
  orderStatus = '';
  paymentMethod = '';

  // Selected Chart settings
  salesInterval: 'daily' | 'weekly' | 'monthly' | 'yearly' = 'daily';
  chartType: 'line' | 'bar' | 'pie' | 'doughnut' | 'area' = 'area';

  // Data payloads
  summary?: DashboardSummary;
  sales?: SalesReport;
  revenue?: RevenueReport;
  orders?: OrdersReport;
  customers?: CustomerReport;
  products?: ProductReport;
  inventory?: InventoryReport;
  payments?: PaymentReport;
  coupons?: CouponReport;
  reviews?: ReviewReport;

  // Chart data mappings
  chartData: { label: string; value: number }[] = [];

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadCategoriesAndBrands();
    this.loadData();
  }

  getPastDate(days: number): string {
    const d = new Date();
    d.setDate(d.getDate() - days);
    return d.toISOString().split('T')[0];
  }

  getTodayDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  loadCategoriesAndBrands(): void {
    this.api.categories().subscribe({
      next: (res) => this.categories = res
    });

    // Populate brands from product list dynamically
    this.api.searchProducts({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        const brandsMap = new Map<string, string>();
        res.items.forEach(p => {
          if (p.brandId && p.brandName) {
            brandsMap.set(p.brandId, p.brandName);
          }
        });
        this.brands = Array.from(brandsMap.entries()).map(([id, name]) => ({ id, name }));
      }
    });
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
    this.errorMsg = '';
    this.notice = '';
    
    // Choose default chart type for different reports
    if (tab === 'sales' || tab === 'revenue') {
      this.chartType = 'area';
    } else if (tab === 'orders' || tab === 'reviews') {
      this.chartType = 'bar';
    } else if (tab === 'payments' || tab === 'inventory') {
      this.chartType = 'doughnut';
    }

    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.errorMsg = '';

    const start = this.startDate || undefined;
    const end = this.endDate || undefined;

    switch (this.activeTab) {
      case 'dashboard':
        this.api.getReportsDashboardSummary().subscribe({
          next: (res) => {
            this.summary = res;
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'sales':
        this.api.getSalesReport(start, end, this.categoryId || undefined, this.brandId || undefined).subscribe({
          next: (res) => {
            this.sales = res;
            this.updateSalesChart();
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'revenue':
        this.api.getRevenueReport(start, end).subscribe({
          next: (res) => {
            this.revenue = res;
            this.chartData = res.revenueTrends.map(r => ({
              label: new Date(r.date).toLocaleDateString(),
              value: r.revenue
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'orders':
        this.api.getOrdersReport(start, end, this.orderStatus || undefined).subscribe({
          next: (res) => {
            this.orders = res;
            // Status distribution chart
            this.chartData = [
              { label: 'Pending', value: res.pendingCount },
              { label: 'Delivered', value: res.deliveredCount },
              { label: 'Cancelled', value: res.cancelledCount },
              { label: 'Returned', value: res.returnedCount }
            ];
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'customers':
        this.api.getCustomerReport(start, end).subscribe({
          next: (res) => {
            this.customers = res;
            this.chartData = res.topCustomers.map(c => ({
              label: c.fullName,
              value: c.totalSpent
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'products':
        this.api.getProductReport(start, end, this.categoryId || undefined, this.brandId || undefined).subscribe({
          next: (res) => {
            this.products = res;
            this.chartData = res.performance.slice(0, 10).map(p => ({
              label: p.productName,
              value: p.revenueGenerated
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'inventory':
        this.api.getInventoryReport().subscribe({
          next: (res) => {
            this.inventory = res;
            this.chartData = [
              { label: 'In Stock', value: res.inStockCount },
              { label: 'Low Stock', value: res.lowStockCount },
              { label: 'Out of Stock', value: res.outOfStockCount }
            ];
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'payments':
        this.api.getPaymentReport(start, end, this.paymentMethod || undefined).subscribe({
          next: (res) => {
            this.payments = res;
            this.chartData = res.methodUsage.map(m => ({
              label: m.method,
              value: m.totalAmount
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'coupons':
        this.api.getCouponReport(start, end).subscribe({
          next: (res) => {
            this.coupons = res;
            this.chartData = res.performance.map(p => ({
              label: p.code,
              value: p.totalSavings
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;

      case 'reviews':
        this.api.getReviewReport(start, end).subscribe({
          next: (res) => {
            this.reviews = res;
            this.chartData = Object.keys(res.ratingDistribution).map(key => ({
              label: key + ' Star',
              value: res.ratingDistribution[+key]
            }));
            this.loading = false;
          },
          error: (err) => this.handleError(err)
        });
        break;
    }
  }

  updateSalesChart(): void {
    if (!this.sales) return;

    let selectedList = this.sales.dailySales;
    if (this.salesInterval === 'weekly') selectedList = this.sales.weeklySales;
    if (this.salesInterval === 'monthly') selectedList = this.sales.monthlySales;
    if (this.salesInterval === 'yearly') selectedList = this.sales.yearlySales;

    this.chartData = selectedList.map(item => ({
      label: item.interval,
      value: item.totalSales
    }));
  }

  export(format: string): void {
    this.exporting = true;
    this.errorMsg = '';
    this.notice = '';

    const start = this.startDate || undefined;
    const end = this.endDate || undefined;

    this.api.exportReport(this.activeTab, format, start, end).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.activeTab}_report_${new Date().toISOString().split('T')[0]}.${format === 'excel' ? 'xls' : format}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.showNotice('Report downloaded successfully!');
        this.exporting = false;
      },
      error: () => {
        this.errorMsg = 'Failed to export report.';
        this.exporting = false;
      }
    });
  }

  private handleError(err: any): void {
    this.errorMsg = err.error?.message || 'Failed to load report data.';
    this.loading = false;
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }
}
