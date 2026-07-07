import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { CheckoutSummary, UserAddressDto, ShippingMethod, Cart, Order, AddAddressRequest } from '../../core/api.models';

@Component({
  selector: 'app-checkout',
  standalone: false,
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss'
})
export class CheckoutComponent implements OnInit {
  cart?: Cart;
  addresses: UserAddressDto[] = [];
  shippingMethods: ShippingMethod[] = [];
  paymentMethods: string[] = [];

  // Selections
  selectedAddressId = '';
  selectedShippingCode = '';
  selectedPaymentMethod = '';
  orderNotes = '';
  deliveryInstructions = '';

  // Pricing calculations from dry-run validation
  subtotal = 0;
  discount = 0;
  shippingCost = 0;
  tax = 0;
  grandTotal = 0;

  // Address Form management
  showAddressForm = false;
  editingAddress: UserAddressDto | null = null;

  // Address Form fields
  addressFullName = '';
  addressMobile = '';
  addressAltMobile = '';
  addressEmail = '';
  addressCountry = 'India';
  addressState = '';
  addressCity = '';
  addressArea = '';
  addressLandmark = '';
  addressPostalCode = '';
  addressLine1 = '';
  addressLine2 = '';
  addressType = 'Home';
  addressIsDefault = false;

  // State flags
  loading = false;
  validating = false;
  placingOrder = false;
  checkoutSuccess = false;
  placedOrder?: Order;

  errorMsg = '';
  notice = '';

  constructor(
    private readonly api: ApiService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.loading = true;
    this.errorMsg = '';
    this.api.getCheckoutSummary().subscribe({
      next: (summary) => {
        this.cart = summary.cart;
        this.addresses = summary.addresses;
        this.shippingMethods = summary.shippingMethods;
        this.paymentMethods = summary.paymentMethods;

        // Set default selections
        const defaultAddr = this.addresses.find(a => a.isDefault) || this.addresses[0];
        if (defaultAddr) this.selectedAddressId = defaultAddr.id;

        if (this.shippingMethods.length > 0) {
          this.selectedShippingCode = this.shippingMethods[0].code;
        }
        if (this.paymentMethods.length > 0) {
          this.selectedPaymentMethod = this.paymentMethods[0];
        }

        this.loading = false;
        this.recalculate();
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = 'Failed to load checkout summary: ' + (err.error || err.message);
      }
    });
  }

  recalculate(): void {
    if (!this.selectedAddressId || !this.selectedShippingCode || !this.selectedPaymentMethod) {
      // Set basic client-side calculations if selections are incomplete
      if (this.cart) {
        this.subtotal = this.cart.subtotal;
        this.discount = this.cart.couponDiscount;
        this.shippingCost = this.selectedShippingCode === 'Express' ? 25 : (this.subtotal >= 100 ? 0 : 10);
        this.tax = (this.subtotal - this.discount) * 0.10;
        this.grandTotal = this.subtotal - this.discount + this.shippingCost + this.tax;
      }
      return;
    }

    this.validating = true;
    this.api.validateCheckout({
      shippingAddressId: this.selectedAddressId,
      shippingMethodCode: this.selectedShippingCode,
      paymentMethod: this.selectedPaymentMethod
    }).subscribe({
      next: (res) => {
        this.validating = false;
        if (res.isValid) {
          this.subtotal = res.subtotal;
          this.discount = res.discount;
          this.shippingCost = res.shippingCost;
          this.tax = res.tax;
          this.grandTotal = res.grandTotal;
        } else {
          this.errorMsg = res.errorMessage || 'Checkout validation failed.';
        }
      },
      error: (err) => {
        this.validating = false;
        this.errorMsg = 'Validation error: ' + (err.error || err.message);
      }
    });
  }

  onAddressChange(): void {
    const addr = this.addresses.find(a => a.id === this.selectedAddressId);
    if (addr && addr.deliveryInstructions) {
      this.deliveryInstructions = addr.deliveryInstructions;
    }
    this.recalculate();
  }

  // Address CRUD inside checkout screen
  openAddAddress(): void {
    this.editingAddress = null;
    this.resetAddressForm();
    this.showAddressForm = true;
  }

  openEditAddress(addr: UserAddressDto): void {
    this.editingAddress = addr;
    this.addressFullName = addr.fullName;
    this.addressMobile = addr.mobileNumber;
    this.addressAltMobile = addr.alternateMobile || '';
    this.addressEmail = addr.email || '';
    this.addressCountry = addr.country;
    this.addressState = addr.state;
    this.addressCity = addr.city;
    this.addressArea = addr.area;
    this.addressLandmark = addr.landmark || '';
    this.addressPostalCode = addr.postalCode;
    this.addressLine1 = addr.addressLine1;
    this.addressLine2 = addr.addressLine2 || '';
    this.addressType = addr.addressType;
    this.addressIsDefault = addr.isDefault;
    this.showAddressForm = true;
  }

  saveAddress(): void {
    if (!this.addressFullName || !this.addressMobile || !this.addressPostalCode || !this.addressLine1 || !this.addressCity || !this.addressState) {
      this.errorMsg = 'Please fill in all required address fields.';
      return;
    }

    const payload: AddAddressRequest = {
      fullName: this.addressFullName,
      mobileNumber: this.addressMobile,
      alternateMobile: this.addressAltMobile || null,
      email: this.addressEmail || null,
      country: this.addressCountry,
      state: this.addressState,
      city: this.addressCity,
      area: this.addressArea,
      landmark: this.addressLandmark || null,
      postalCode: this.addressPostalCode,
      addressLine1: this.addressLine1,
      addressLine2: this.addressLine2 || null,
      addressType: this.addressType,
      deliveryInstructions: this.deliveryInstructions || null,
      isDefault: this.addressIsDefault
    };

    const action = this.editingAddress
      ? this.api.checkoutUpdateAddress(this.editingAddress.id, payload)
      : this.api.checkoutAddAddress(payload);

    action.subscribe({
      next: (addr) => {
        this.showAddressForm = false;
        this.showNotice('Address saved successfully.');
        this.loadSummary();
      },
      error: (err) => this.errorMsg = 'Failed to save address: ' + (err.error || err.message)
    });
  }

  deleteAddress(id: string): void {
    if (!confirm('Are you sure you want to delete this address?')) return;
    this.api.checkoutDeleteAddress(id).subscribe({
      next: () => {
        this.showNotice('Address deleted.');
        this.loadSummary();
      },
      error: (err) => this.errorMsg = 'Failed to delete address: ' + (err.error || err.message)
    });
  }

  setDefaultAddress(id: string): void {
    this.api.checkoutSetDefaultAddress(id).subscribe({
      next: () => {
        this.showNotice('Default address updated.');
        this.loadSummary();
      },
      error: (err) => this.errorMsg = 'Failed to set default: ' + (err.error || err.message)
    });
  }

  resetAddressForm(): void {
    this.addressFullName = '';
    this.addressMobile = '';
    this.addressAltMobile = '';
    this.addressEmail = '';
    this.addressCountry = 'India';
    this.addressState = '';
    this.addressCity = '';
    this.addressArea = '';
    this.addressLandmark = '';
    this.addressPostalCode = '';
    this.addressLine1 = '';
    this.addressLine2 = '';
    this.addressType = 'Home';
    this.addressIsDefault = false;
  }

  placeOrder(): void {
    if (!this.selectedAddressId) {
      this.errorMsg = 'Please select a delivery address.';
      return;
    }

    this.placingOrder = true;
    this.errorMsg = '';

    this.api.placeOrder({
      shippingAddressId: this.selectedAddressId,
      shippingMethodCode: this.selectedShippingCode,
      paymentMethod: this.selectedPaymentMethod,
      orderNotes: this.orderNotes || null,
      deliveryInstructions: this.deliveryInstructions || null
    }).subscribe({
      next: (order) => {
        this.placingOrder = false;
        this.placedOrder = order;

        if (this.selectedPaymentMethod === 'CashOnDelivery') {
          this.checkoutSuccess = true;
          void this.router.navigate(['/order-success'], {
            queryParams: {
              orderNumber: order.orderNumber,
              estimatedDelivery: order.estimatedDeliveryDate
            }
          });
        } else {
          // Initialize online payment and redirect to payment portal
          this.api.initializePayment({
            orderId: order.id,
            provider: this.selectedPaymentMethod,
            currency: 'USD'
          }).subscribe({
            next: (payRes) => {
              void this.router.navigate(['/payment'], {
                queryParams: {
                  orderId: order.id,
                  paymentId: payRes.paymentId,
                  provider: this.selectedPaymentMethod,
                  amount: order.totalAmount,
                  currency: 'USD'
                }
              });
            },
            error: (err) => {
              this.errorMsg = 'Failed to initialize payment: ' + (err.error || err.message);
            }
          });
        }
      },
      error: (err) => {
        this.placingOrder = false;
        this.errorMsg = 'Failed to place order: ' + (err.error || err.message);
      }
    });
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }

  image(item: any): string {
    return item.imageUrl || 'https://images.unsplash.com/photo-1557821552-17105176677c?auto=format&fit=crop&w=900&q=80';
  }
}
