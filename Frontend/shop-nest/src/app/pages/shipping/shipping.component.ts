import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { Shipment, Courier, ShipmentTracking } from '../../core/api.models';

@Component({
  selector: 'app-shipping',
  standalone: false,
  templateUrl: './shipping.component.html',
  styleUrl: './shipping.component.scss'
})
export class ShippingComponent implements OnInit {
  // Tabs
  activeTab: 'shipments' | 'couriers' = 'shipments';
  isAdmin = false;

  // Data Arrays
  shipments: Shipment[] = [];
  couriers: Courier[] = [];
  
  // Selection
  selectedShipment?: Shipment;
  timeline: ShipmentTracking[] = [];
  
  // Search & Filter
  searchQuery = '';
  courierFilter = '';
  statusFilter = '';
  
  page = 1;
  totalItems = 0;
  
  // Modals / Dialogs
  showCreateShipment = false;
  showAssignCourier = false;
  showUpdateStatus = false;
  showReschedule = false;
  showFail = false;
  showCreateCourier = false;

  // Form Fields
  orderId = '';
  shippingAddress = '';
  billingAddress = '';
  shippingCharges = 0;
  deliveryInstructions = '';
  manualTrackingNumber = '';

  selectedCourierId = '';
  newCourierName = '';
  newCourierCode = '';
  newCourierContact = '';
  newCourierWebsite = '';
  newCourierTime = '2-5 Business Days';

  trackingStatus = '';
  trackingLocation = '';
  trackingDescription = '';
  
  rescheduleDate = '';
  failReason = '';

  // Messages
  errorMsg = '';
  notice = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.isAdmin = this.api.currentUser()?.role === 'Admin';
    this.loadShipments();
    this.loadCouriers();
  }

  loadShipments(): void {
    this.errorMsg = '';
    this.api.getShipments(this.courierFilter || undefined, this.statusFilter || undefined, this.page).subscribe({
      next: (res) => {
        this.shipments = res.items;
        this.totalItems = res.totalCount;
        this.filterShipmentsLocally();
      },
      error: (err) => this.errorMsg = 'Failed to load shipments: ' + (err.error || err.message)
    });
  }

  loadCouriers(): void {
    this.api.getCouriers().subscribe({
      next: (c) => this.couriers = c,
      error: (err) => console.error('Failed to load couriers:', err)
    });
  }

  filterShipmentsLocally(): void {
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      this.shipments = this.shipments.filter(s => 
        s.shipmentNumber.toLowerCase().includes(q) ||
        (s.trackingNumber && s.trackingNumber.toLowerCase().includes(q)) ||
        s.orderNumber.toLowerCase().includes(q) ||
        s.shippingAddress.toLowerCase().includes(q)
      );
    }
  }

  selectShipment(shipment: Shipment): void {
    this.selectedShipment = shipment;
    this.timeline = [];
    this.api.getShipmentTimeline(shipment.id).subscribe({
      next: (t) => this.timeline = t,
      error: (err) => console.error('Failed to load shipment timeline:', err)
    });
  }

  switchTab(tab: 'shipments' | 'couriers'): void {
    this.activeTab = tab;
    this.selectedShipment = undefined;
    this.page = 1;
    if (tab === 'shipments') {
      this.loadShipments();
    } else {
      this.loadCouriers();
    }
  }

  // Dialog Toggles
  openCreateShipment(): void {
    this.orderId = '';
    this.shippingAddress = '';
    this.billingAddress = '';
    this.shippingCharges = 10;
    this.deliveryInstructions = '';
    this.manualTrackingNumber = '';
    this.selectedCourierId = '';
    this.showCreateShipment = true;
  }

  openAssignCourier(): void {
    if (!this.selectedShipment) return;
    this.selectedCourierId = this.selectedShipment.courier?.id || '';
    this.manualTrackingNumber = this.selectedShipment.trackingNumber || '';
    this.showAssignCourier = true;
  }

  openUpdateStatus(): void {
    this.trackingStatus = this.selectedShipment?.status || '';
    this.trackingLocation = '';
    this.trackingDescription = '';
    this.showUpdateStatus = true;
  }

  openReschedule(): void {
    this.rescheduleDate = '';
    this.failReason = '';
    this.showReschedule = true;
  }

  openFail(): void {
    this.failReason = '';
    this.showFail = true;
  }

  openCreateCourier(): void {
    this.newCourierName = '';
    this.newCourierCode = '';
    this.newCourierContact = '';
    this.newCourierWebsite = '';
    this.newCourierTime = '2-5 Business Days';
    this.showCreateCourier = true;
  }

  closeDialogs(): void {
    this.showCreateShipment = false;
    this.showAssignCourier = false;
    this.showUpdateStatus = false;
    this.showReschedule = false;
    this.showFail = false;
    this.showCreateCourier = false;
  }

  // Mutator Actions
  submitCreateShipment(): void {
    if (!this.orderId || !this.shippingAddress || !this.billingAddress) return;
    this.api.createShipment({
      orderId: this.orderId,
      courierId: this.selectedCourierId || null,
      manualTrackingNumber: this.manualTrackingNumber || null,
      shippingAddress: this.shippingAddress,
      billingAddress: this.billingAddress,
      shippingCharges: this.shippingCharges,
      deliveryInstructions: this.deliveryInstructions || null
    }).subscribe({
      next: (s) => {
        this.showNotice('Shipment created successfully.');
        this.selectShipment(s);
        this.loadShipments();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Failed to create shipment: ' + (err.error || err.message)
    });
  }

  submitAssignCourier(): void {
    if (!this.selectedShipment || !this.selectedCourierId) return;
    this.api.assignShipmentCourier(this.selectedShipment.id, {
      courierId: this.selectedCourierId,
      manualTrackingNumber: this.manualTrackingNumber || null
    }).subscribe({
      next: (s) => {
        this.showNotice('Courier assigned.');
        this.selectShipment(s);
        this.loadShipments();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Failed to assign courier: ' + (err.error || err.message)
    });
  }

  submitUpdateStatus(): void {
    if (!this.selectedShipment || !this.trackingStatus || !this.trackingLocation) return;
    this.api.updateShipmentStatus(this.selectedShipment.id, {
      status: this.trackingStatus,
      location: this.trackingLocation,
      description: this.trackingDescription || null
    }).subscribe({
      next: (s) => {
        this.showNotice('Shipment status updated.');
        this.selectShipment(s);
        this.loadShipments();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Failed to update status: ' + (err.error || err.message)
    });
  }

  submitReschedule(): void {
    if (!this.selectedShipment || !this.rescheduleDate || !this.failReason) return;
    this.api.rescheduleDelivery(this.selectedShipment.id, {
      newEstimatedDeliveryDate: this.rescheduleDate,
      reason: this.failReason
    }).subscribe({
      next: (s) => {
        this.showNotice('Delivery rescheduled.');
        this.selectShipment(s);
        this.loadShipments();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Reschedule failed: ' + (err.error || err.message)
    });
  }

  submitDeliver(): void {
    if (!this.selectedShipment) return;
    if (!confirm('Mark this shipment as successfully delivered?')) return;
    this.api.markShipmentDelivered(this.selectedShipment.id).subscribe({
      next: (s) => {
        this.showNotice('Shipment marked as Delivered.');
        this.selectShipment(s);
        this.loadShipments();
      },
      error: (err) => this.errorMsg = 'Failed to deliver: ' + (err.error || err.message)
    });
  }

  submitFail(): void {
    if (!this.selectedShipment || !this.failReason) return;
    this.api.markShipmentFailed(this.selectedShipment.id, this.failReason).subscribe({
      next: (s) => {
        this.showNotice('Shipment delivery failed.');
        this.selectShipment(s);
        this.loadShipments();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Failed to mark fail: ' + (err.error || err.message)
    });
  }

  submitCreateCourier(): void {
    if (!this.newCourierName || !this.newCourierCode) return;
    this.api.createCourier({
      name: this.newCourierName,
      code: this.newCourierCode,
      contact: this.newCourierContact,
      website: this.newCourierWebsite,
      estimatedDeliveryTime: this.newCourierTime
    }).subscribe({
      next: () => {
        this.showNotice('New Courier added.');
        this.loadCouriers();
        this.closeDialogs();
      },
      error: (err) => this.errorMsg = 'Failed to add courier: ' + (err.error || err.message)
    });
  }

  // Helpers
  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }
}
