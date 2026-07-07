import { Component, OnInit } from '@angular/core';
import { Inventory, InventoryTransaction, Warehouse, Category } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-inventory',
  standalone: false,
  templateUrl: './admin-inventory.component.html',
  styleUrl: './admin-inventory.component.scss'
})
export class AdminInventoryComponent implements OnInit {
  // Tabs: 'list', 'history', 'alerts'
  activeTab: 'list' | 'history' | 'alerts' = 'list';

  // Data states
  inventoryItems: Inventory[] = [];
  transactions: InventoryTransaction[] = [];
  warehouses: Warehouse[] = [];
  categories: Category[] = [];

  // Summary widgets
  totalProducts = 0;
  totalStockValue = 0;
  lowStockCount = 0;
  outOfStockCount = 0;

  // Pagination & Filters (Inventory List)
  search = '';
  filterWarehouse = '';
  filterCategory = '';
  filterStockStatus = '';
  sortBy = 'lastupdated';
  descending = true;
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Pagination & Filters (Transactions)
  txSearch = '';
  txType = '';
  txWarehouse = '';
  txStartDate = '';
  txEndDate = '';
  txPage = 1;
  txPageSize = 10;
  txTotalCount = 0;
  txTotalPages = 0;

  // Overlay Modals
  showStockInModal = false;
  showStockOutModal = false;
  showAdjustModal = false;
  showLimitsModal = false;
  selectedItem?: Inventory;

  // Modal form payloads
  stockInForm = {
    quantity: 1,
    unitCost: 0,
    reason: '',
    referenceNumber: ''
  };

  stockOutForm = {
    quantity: 1,
    reason: '',
    referenceNumber: ''
  };

  adjustForm = {
    newQuantity: 0,
    reason: '',
    referenceNumber: ''
  };

  limitsForm = {
    minimumStockLevel: 5,
    maximumStockLevel: 100,
    reorderLevel: 10
  };

  notice = '';
  errorMsg = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadCategories();
    this.loadSummaryStats();
    this.loadInventory();
  }

  // Loaders
  loadWarehouses(): void {
    this.api.getWarehouses().subscribe({
      next: (res) => this.warehouses = res
    });
  }

  loadCategories(): void {
    this.api.getFeaturedProducts(1).subscribe({
      next: () => {
        // Feed categories via category helper
        this.api.searchProducts({ page: 1, pageSize: 1 }).subscribe();
      }
    });
  }

  loadSummaryStats(): void {
    // Call low stock count endpoint directly
    this.api.getLowStockProducts(1, 1).subscribe({
      next: (res) => this.lowStockCount = res.totalCount
    });

    this.api.getOutOfStockProducts(1, 1).subscribe({
      next: (res) => this.outOfStockCount = res.totalCount
    });

    // Summing value over list
    this.api.searchInventory({ page: 1, pageSize: 1000 }).subscribe({
      next: (res) => {
        this.totalProducts = res.totalCount;
        this.totalStockValue = res.items.reduce((sum, item) => sum + (item.currentStock * item.unitCost), 0);
      }
    });
  }

  loadInventory(): void {
    const filters: any = {
      page: this.page,
      pageSize: this.pageSize,
      query: this.search,
      sortBy: this.sortBy,
      sortDescending: this.descending
    };

    if (this.filterWarehouse) filters.warehouseId = this.filterWarehouse;
    if (this.filterCategory) filters.categoryId = this.filterCategory;
    if (this.filterStockStatus) filters.stockStatus = this.filterStockStatus;

    this.api.searchInventory(filters).subscribe({
      next: (res) => {
        this.inventoryItems = res.items;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages;
      },
      error: (err) => this.errorMsg = 'Failed to load inventory: ' + (err.error || err.message)
    });
  }

  loadTransactions(): void {
    const filters: any = {
      page: this.txPage,
      pageSize: this.txPageSize,
      query: this.txSearch,
      transactionType: this.txType
    };

    if (this.txWarehouse) filters.warehouseId = this.txWarehouse;
    if (this.txStartDate) filters.startDate = new Date(this.txStartDate).toISOString();
    if (this.txEndDate) filters.endDate = new Date(this.txEndDate).toISOString();

    this.api.getInventoryTransactions(filters).subscribe({
      next: (res) => {
        this.transactions = res.items;
        this.txTotalCount = res.totalCount;
        this.txTotalPages = res.totalPages;
      },
      error: (err) => this.errorMsg = 'Failed to load transaction history: ' + (err.error || err.message)
    });
  }

  // Tab switcher
  switchTab(tab: 'list' | 'history' | 'alerts'): void {
    this.activeTab = tab;
    this.errorMsg = '';
    this.notice = '';
    if (tab === 'list') {
      this.loadInventory();
    } else if (tab === 'history') {
      this.loadTransactions();
    } else if (tab === 'alerts') {
      // Load only low stock items
      this.filterStockStatus = 'LowStock';
      this.loadInventory();
    }
  }

  // Filtering & Search
  onSearch(): void {
    this.page = 1;
    this.loadInventory();
  }

  onTxSearch(): void {
    this.txPage = 1;
    this.loadTransactions();
  }

  clearFilters(): void {
    this.search = '';
    this.filterWarehouse = '';
    this.filterCategory = '';
    this.filterStockStatus = '';
    this.page = 1;
    this.loadInventory();
  }

  // Paging
  changePage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.page = p;
      this.loadInventory();
    }
  }

  changeTxPage(p: number): void {
    if (p >= 1 && p <= this.txTotalPages) {
      this.txPage = p;
      this.loadTransactions();
    }
  }

  // Modals actions
  openStockIn(item: Inventory): void {
    this.selectedItem = item;
    this.stockInForm = {
      quantity: 10,
      unitCost: item.unitCost || 0,
      reason: 'Purchase Restock',
      referenceNumber: ''
    };
    this.showStockInModal = true;
  }

  openStockOut(item: Inventory): void {
    this.selectedItem = item;
    this.stockOutForm = {
      quantity: 1,
      reason: 'Manual Stock-Out',
      referenceNumber: ''
    };
    this.showStockOutModal = true;
  }

  openAdjust(item: Inventory): void {
    this.selectedItem = item;
    this.adjustForm = {
      newQuantity: item.currentStock,
      reason: 'Physical Count Adjustment',
      referenceNumber: ''
    };
    this.showAdjustModal = true;
  }

  openLimits(item: Inventory): void {
    this.selectedItem = item;
    this.limitsForm = {
      minimumStockLevel: item.minimumStockLevel,
      maximumStockLevel: item.maximumStockLevel,
      reorderLevel: item.reorderLevel
    };
    this.showLimitsModal = true;
  }

  // Modal Submissions
  submitStockIn(): void {
    if (!this.selectedItem) return;
    this.api.stockIn({
      productId: this.selectedItem.productId,
      productVariantId: this.selectedItem.productVariantId,
      warehouseId: this.selectedItem.warehouseId,
      quantity: this.stockInForm.quantity,
      unitCost: this.stockInForm.unitCost,
      reason: this.stockInForm.reason,
      referenceNumber: this.stockInForm.referenceNumber
    }).subscribe({
      next: () => {
        this.showNotice('Stock added successfully.');
        this.showStockInModal = false;
        this.refreshData();
      },
      error: (err) => this.errorMsg = 'Stock-In failed: ' + (err.error || err.message)
    });
  }

  submitStockOut(): void {
    if (!this.selectedItem) return;
    this.api.stockOut({
      productId: this.selectedItem.productId,
      productVariantId: this.selectedItem.productVariantId,
      warehouseId: this.selectedItem.warehouseId,
      quantity: this.stockOutForm.quantity,
      reason: this.stockOutForm.reason,
      referenceNumber: this.stockOutForm.referenceNumber
    }).subscribe({
      next: () => {
        this.showNotice('Stock dispatched successfully.');
        this.showStockOutModal = false;
        this.refreshData();
      },
      error: (err) => this.errorMsg = 'Stock-Out failed: ' + (err.error || err.message)
    });
  }

  submitAdjustment(): void {
    if (!this.selectedItem) return;
    this.api.adjustStock({
      productId: this.selectedItem.productId,
      productVariantId: this.selectedItem.productVariantId,
      warehouseId: this.selectedItem.warehouseId,
      newQuantity: this.adjustForm.newQuantity,
      reason: this.adjustForm.reason,
      referenceNumber: this.adjustForm.referenceNumber
    }).subscribe({
      next: () => {
        this.showNotice('Stock adjusted successfully.');
        this.showAdjustModal = false;
        this.refreshData();
      },
      error: (err) => this.errorMsg = 'Adjustment failed: ' + (err.error || err.message)
    });
  }

  submitLimits(): void {
    if (!this.selectedItem) return;
    this.api.updateInventoryLimits({
      productId: this.selectedItem.productId,
      productVariantId: this.selectedItem.productVariantId,
      warehouseId: this.selectedItem.warehouseId,
      minimumStockLevel: this.limitsForm.minimumStockLevel,
      maximumStockLevel: this.limitsForm.maximumStockLevel,
      reorderLevel: this.limitsForm.reorderLevel
    }).subscribe({
      next: () => {
        this.showNotice('Stock alerts limits updated.');
        this.showLimitsModal = false;
        this.refreshData();
      },
      error: (err) => this.errorMsg = 'Limits update failed: ' + (err.error || err.message)
    });
  }

  refreshData(): void {
    this.loadSummaryStats();
    if (this.activeTab === 'list' || this.activeTab === 'alerts') {
      this.loadInventory();
    } else {
      this.loadTransactions();
    }
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }

  // Reports Exports (Excel / CSV)
  exportToCsv(): void {
    let headers = 'Product,SKU,Warehouse,Current Stock,Available,Reserved,Min Alert,Max Alert,Unit Cost,Selling Price,Value ($)\r\n';
    
    // Fetch all for export
    this.api.searchInventory({ page: 1, pageSize: 1000 }).subscribe({
      next: (res) => {
        let csv = headers;
        res.items.forEach(x => {
          const varName = x.productVariantName ? ` (${x.productVariantName})` : '';
          const name = `"${x.productName}${varName}"`;
          const val = x.currentStock * x.unitCost;
          csv += `${name},${x.sku},"${x.warehouseName || 'Main'}",${x.currentStock},${x.availableStock},${x.reservedStock},${x.minimumStockLevel},${x.maximumStockLevel},${x.unitCost},${x.sellingPrice},${val}\r\n`;
        });

        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.setAttribute('download', `inventory_report_${new Date().toISOString().slice(0,10)}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
    });
  }

  exportTransactionsToCsv(): void {
    let headers = 'Transaction Number,Date,Product,SKU,Quantity,Prev Stock,New Stock,Type,Reason,Performed By,Reference\r\n';
    this.api.getInventoryTransactions({ page: 1, pageSize: 2000 }).subscribe({
      next: (res) => {
        let csv = headers;
        res.items.forEach(x => {
          const varName = x.variantName ? ` (${x.variantName})` : '';
          const name = `"${x.productName}${varName}"`;
          csv += `${x.transactionNumber},${x.transactionDate},${name},${x.sku},${x.quantity},${x.previousStock},${x.updatedStock},${x.transactionType},"${x.reason || ''}",${x.performedBy},${x.referenceNumber || ''}\r\n`;
        });

        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.setAttribute('download', `transactions_history_${new Date().toISOString().slice(0,10)}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
    });
  }

  printReport(): void {
    window.print();
  }
}
