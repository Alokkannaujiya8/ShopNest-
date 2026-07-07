import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Order, OrderStatusHistory, OrderTracking, OrderStatus } from '../../core/api.models';
import { ApiService } from '../../core/api.service';
import { SignalrService } from '../../core/signalr.service';

@Component({
  selector: 'app-orders',
  standalone: false,
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit, OnDestroy {
  // Tabs
  activeTab: 'my' | 'admin' = 'my';
  isAdmin = false;

  // Lists
  myOrdersList: Order[] = [];
  adminOrdersList: Order[] = [];
  
  // Pagination / Search / Filters
  searchQuery = '';
  statusFilter = '';
  paymentStatusFilter = '';
  sortField = 'date';
  sortOrder: 'asc' | 'desc' = 'desc';
  
  page = 1;
  pageSize = 10;
  totalItems = 0;
  
  // Selection
  selectedOrder?: Order;
  timeline: OrderStatusHistory[] = [];
  trackingHistory: OrderTracking[] = [];
  
  // Dialogs / States
  showCancelDialog = false;
  showReturnDialog = false;
  showRefundDialog = false;
  showCourierDialog = false;
  showTrackingDialog = false;
  showStatusDialog = false;
  
  reason = '';
  courierPartner = '';
  trackingNumber = '';
  trackingStatus = '';
  trackingLocation = '';
  selectedStatus: OrderStatus = 'Pending';

  // Notices
  errorMsg = '';
  notice = '';
  
  paymentMessage = '';
  activeSession?: { paymentId: string; providerOrderId: string; orderNumber: string };
  private sub?: Subscription;

  constructor(
    readonly api: ApiService,
    readonly signalr: SignalrService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.api.currentUser()?.role === 'Admin';
    if (this.isAdmin) {
      this.activeTab = 'admin';
    }
    this.loadOrders();

    this.route.paramMap.subscribe(params => {
      const orderId = params.get('orderId');
      const isTracking = this.router.url.includes('tracking');
      
      if (orderId) {
        this.api.getOrderById(orderId, null).subscribe({
          next: (order) => {
            if (order) {
              this.selectOrder(order);
              if (isTracking) {
                this.showTrackingDialog = true;
              }
            }
          }
        });
      }
    });

    this.sub = this.signalr.orderStatusChanged$.subscribe((updatedOrder: any) => {
      this.showNotice(`Order ${updatedOrder.orderNumber} status changed to ${updatedOrder.status}`);
      this.loadOrders();
      if (this.selectedOrder && this.selectedOrder.id === updatedOrder.id) {
        this.selectOrder(updatedOrder);
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  loadOrders(): void {
    this.errorMsg = '';
    if (this.activeTab === 'my') {
      this.api.myOrders(this.page).subscribe({
        next: (res) => {
          this.myOrdersList = res.items;
          this.totalItems = res.totalCount;
        },
        error: (err) => this.errorMsg = 'Failed to load customer orders: ' + (err.error || err.message)
      });
    } else {
      this.api.allOrders(this.page).subscribe({
        next: (res) => {
          this.adminOrdersList = res.items;
          this.totalItems = res.totalCount;
          this.filterAndSortAdminOrders();
        },
        error: (err) => this.errorMsg = 'Failed to load administrator orders: ' + (err.error || err.message)
      });
    }
  }

  filterAndSortAdminOrders(): void {
    let filtered = [...this.adminOrdersList];

    // Client-side search & filter simulation for immediate UX response
    if (this.searchQuery) {
      const query = this.searchQuery.toLowerCase();
      filtered = filtered.filter(o => 
        o.orderNumber.toLowerCase().includes(query) ||
        o.shippingAddress.toLowerCase().includes(query) ||
        (o.courierPartner && o.courierPartner.toLowerCase().includes(query)) ||
        (o.trackingNumber && o.trackingNumber.toLowerCase().includes(query))
      );
    }

    if (this.statusFilter) {
      filtered = filtered.filter(o => o.status === this.statusFilter);
    }

    if (this.paymentStatusFilter) {
      filtered = filtered.filter(o => o.payment?.status === this.paymentStatusFilter);
    }

    // Sort
    filtered.sort((a, b) => {
      let valA: any = a.id;
      let valB: any = b.id;

      if (this.sortField === 'date') {
        valA = a.id; // Or order date if stored. We can sort by orderNumber or id
        valB = b.id;
      } else if (this.sortField === 'amount') {
        valA = a.totalAmount;
        valB = b.totalAmount;
      }

      if (valA < valB) return this.sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortOrder === 'asc' ? 1 : -1;
      return 0;
    });

    this.adminOrdersList = filtered;
  }

  switchTab(tab: 'my' | 'admin'): void {
    this.activeTab = tab;
    this.selectedOrder = undefined;
    this.page = 1;
    this.loadOrders();
  }

  selectOrder(order: Order): void {
    this.selectedOrder = order;
    this.timeline = [];
    this.trackingHistory = [];
    this.errorMsg = '';

    this.api.getOrderTimeline(order.id).subscribe({
      next: (t) => this.timeline = t,
      error: (err) => console.error('Failed to load timeline:', err)
    });

    this.api.getOrderTracking(order.id).subscribe({
      next: (tr) => this.trackingHistory = tr,
      error: (err) => console.error('Failed to load tracking:', err)
    });
  }

  // Invoice & Receipt Downloads
  downloadInvoiceFile(order: Order): void {
    const url = this.api.getInvoiceDownloadUrl(order.id);
    window.open(url, '_blank');
  }

  downloadReceiptFile(order: Order): void {
    // Generate text/plain receipt client side
    let receipt = `SHOPNEST RECEIPT\n`;
    receipt += `Order: ${order.orderNumber}\n`;
    receipt += `Total Amount: $${order.totalAmount}\n`;
    receipt += `Status: ${order.status}\n`;
    receipt += `Items:\n`;
    order.items.forEach(i => {
      receipt += `- ${i.productName} x ${i.quantity} @ $${i.unitPrice}\n`;
    });
    
    const blob = new Blob([receipt], { type: 'text/plain;charset=utf-8' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `Receipt_${order.orderNumber}.txt`;
    link.click();
  }

  // Buy Again / Reorder
  buyAgain(order: Order): void {
    this.errorMsg = '';
    if (order.items && order.items.length > 0) {
      const first = order.items[0];
      this.api.addToCart(first.productId, first.quantity).subscribe({
        next: () => this.showNotice(`Added '${first.productName}' back to cart!`),
        error: (err) => this.errorMsg = 'Failed to add item back to cart: ' + (err.error || err.message)
      });
    }
  }

  // Dialog triggers
  openCancel(): void { this.reason = ''; this.showCancelDialog = true; }
  openReturn(): void { this.reason = ''; this.showReturnDialog = true; }
  openRefund(): void { this.reason = ''; this.showRefundDialog = true; }
  openCourier(): void {
    this.courierPartner = this.selectedOrder?.courierPartner || '';
    this.trackingNumber = this.selectedOrder?.trackingNumber || '';
    this.showCourierDialog = true;
  }
  openStatus(): void {
    this.selectedStatus = this.selectedOrder?.status || 'Pending';
    this.showStatusDialog = true;
  }
  openTracking(): void {
    this.trackingStatus = '';
    this.trackingLocation = '';
    this.showTrackingDialog = true;
  }

  closeDialogs(): void {
    this.showCancelDialog = false;
    this.showReturnDialog = false;
    this.showRefundDialog = false;
    this.showCourierDialog = false;
    this.showStatusDialog = false;
    this.showTrackingDialog = false;
  }

  // API Mutators
  submitCancel(): void {
    if (!this.selectedOrder || !this.reason) return;
    this.api.cancelOrder(this.selectedOrder.id, this.reason).subscribe({
      next: (ord) => {
        this.showNotice('Order cancelled successfully.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Cancellation failed: ' + (err.error || err.message)
    });
  }

  submitReturn(): void {
    if (!this.selectedOrder || !this.reason) return;
    this.api.requestReturn(this.selectedOrder.id, this.reason).subscribe({
      next: (ord) => {
        this.showNotice('Return request submitted.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Return request failed: ' + (err.error || err.message)
    });
  }

  submitRefund(): void {
    if (!this.selectedOrder || !this.reason) return;
    this.api.requestRefund(this.selectedOrder.id, this.reason).subscribe({
      next: (ord) => {
        this.showNotice('Refund request submitted.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Refund request failed: ' + (err.error || err.message)
    });
  }

  submitCourier(): void {
    if (!this.selectedOrder || !this.courierPartner || !this.trackingNumber) return;
    this.api.assignCourier(this.selectedOrder.id, this.courierPartner, this.trackingNumber).subscribe({
      next: (ord) => {
        this.showNotice('Courier details saved.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Assign courier failed: ' + (err.error || err.message)
    });
  }

  submitStatus(): void {
    if (!this.selectedOrder) return;
    this.api.updateOrderStatus(this.selectedOrder.id, this.selectedStatus).subscribe({
      next: (ord) => {
        this.showNotice('Order status updated.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Update status failed: ' + (err.error || err.message)
    });
  }

  submitTrackingUpdate(): void {
    if (!this.selectedOrder || !this.trackingStatus || !this.trackingLocation) return;
    this.api.addTrackingUpdate(this.selectedOrder.id, this.trackingStatus, this.trackingLocation).subscribe({
      next: (ord) => {
        this.showNotice('Tracking update added.');
        this.selectOrder(ord);
        this.loadOrders();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Add tracking update failed: ' + (err.error || err.message)
    });
  }

  // Payment simulator triggers
  pay(order: Order): void {
    this.api.createPayment(order.id).subscribe((session) => {
      this.activeSession = {
        paymentId: session.paymentId,
        providerOrderId: session.providerOrderId,
        orderNumber: order.orderNumber
      };
      this.paymentMessage = `Payment session ready for Order ${order.orderNumber}.`;
    });
  }

  simulateSuccess(): void {
    if (!this.activeSession) return;
    this.api.completePayment(this.activeSession.paymentId, 'Succeeded').subscribe({
      next: () => {
        this.paymentMessage = `Simulated Payment Succeeded for Order ${this.activeSession?.orderNumber}!`;
        this.activeSession = undefined;
        this.loadOrders();
      },
      error: () => this.paymentMessage = 'Simulation failed.'
    });
  }

  simulateFailure(): void {
    if (!this.activeSession) return;
    this.api.completePayment(this.activeSession.paymentId, 'Failed').subscribe({
      next: () => {
        this.paymentMessage = `Simulated Payment Failed for Order ${this.activeSession?.orderNumber}.`;
        this.activeSession = undefined;
        this.loadOrders();
      },
      error: () => this.paymentMessage = 'Simulation failed.'
    });
  }

  // Helpers
  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 4000);
  }
}
