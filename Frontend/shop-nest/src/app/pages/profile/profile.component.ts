import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Order, Product, UserAddressDto, UserProfileDto } from '../../core/api.models';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  activeTab: 'profile' | 'addresses' | 'orders' | 'wishlist' | 'password' | 'reviews' = 'profile';
  loading = false;
  message = '';
  error = '';

  // Profile data
  profileData: UserProfileDto | null = null;
  isEditingProfile = false;
  editFullName = '';
  editMobileNumber = '';
  editDateOfBirth = '';
  editGender = '';
  editBio = '';

  // Address data
  addressesList: UserAddressDto[] = [];
  isAddressModalOpen = false;
  isEditingAddress = false;
  editingAddressId: string | null = null;

  // Address form fields
  addrFullName = '';
  addrMobileNumber = '';
  addrAlternateMobile = '';
  addrCountry = 'India';
  addrState = '';
  addrCity = '';
  addrArea = '';
  addrLandmark = '';
  addrPostalCode = '';
  addrAddressLine1 = '';
  addrAddressLine2 = '';
  addrAddressType = 'Home';
  addrIsDefault = false;

  // Password data
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  strengthScore = 0;
  strengthLabel = 'Too Weak';
  strengthClass = 'strength-0';

  // Orders and Wishlist
  ordersList: Order[] = [];
  expandedOrderId: string | null = null;
  wishlistItems: Product[] = [];

  constructor(
    public readonly api: ApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const url = this.router.url;
    if (url.includes('addresses')) {
      this.activeTab = 'addresses';
    } else if (url.includes('change-password')) {
      this.activeTab = 'password';
    } else if (url.includes('my-reviews')) {
      this.activeTab = 'reviews';
    } else if (url.includes('orders')) {
      this.activeTab = 'orders';
    } else if (url.includes('wishlist')) {
      this.activeTab = 'wishlist';
    } else {
      this.activeTab = 'profile';
    }

    this.loadProfile();
    if (this.activeTab === 'addresses') this.loadAddresses();
    if (this.activeTab === 'orders') this.loadOrders();
    if (this.activeTab === 'wishlist') this.loadWishlist();
    if (this.activeTab === 'reviews') this.loadReviews();
  }

  // ==========================================
  // TAB NAVIGATION
  // ==========================================
  switchTab(tab: 'profile' | 'addresses' | 'orders' | 'wishlist' | 'password' | 'reviews'): void {
    this.activeTab = tab;
    this.error = '';
    this.message = '';
    
    if (tab === 'profile') this.loadProfile();
    else if (tab === 'addresses') this.loadAddresses();
    else if (tab === 'orders') this.loadOrders();
    else if (tab === 'wishlist') this.loadWishlist();
    else if (tab === 'reviews') this.loadReviews();
  }

  // ==========================================
  // PROFILE MANAGEMENT
  // ==========================================
  loadProfile(): void {
    this.loading = true;
    this.api.profile().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.profileData = res.data;
          this.resetProfileEditForm();
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  resetProfileEditForm(): void {
    if (!this.profileData) return;
    this.editFullName = this.profileData.fullName;
    this.editMobileNumber = this.profileData.mobileNumber;
    this.editBio = this.profileData.bio || '';
    this.editGender = this.profileData.gender || 'Male';
    
    if (this.profileData.dateOfBirth) {
      // Format DateTime to YYYY-MM-DD
      this.editDateOfBirth = new Date(this.profileData.dateOfBirth).toISOString().split('T')[0];
    } else {
      this.editDateOfBirth = '';
    }
  }

  startEditingProfile(): void {
    this.isEditingProfile = true;
    this.resetProfileEditForm();
  }

  cancelEditingProfile(): void {
    this.isEditingProfile = false;
    this.error = '';
  }

  saveProfile(): void {
    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.updateProfile({
      fullName: this.editFullName,
      mobileNumber: this.editMobileNumber,
      dateOfBirth: this.editDateOfBirth ? new Date(this.editDateOfBirth).toISOString() : null,
      gender: this.editGender,
      bio: this.editBio
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.profileData = res.data;
          this.isEditingProfile = false;
          this.message = 'Profile details updated successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  onProfilePictureSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    
    // Client-side validations
    const allowedExtensions = ['image/jpeg', 'image/png', 'image/gif', 'image/jpg'];
    if (!allowedExtensions.includes(file.type)) {
      this.error = 'Only JPG, JPEG, PNG, and GIF images are allowed.';
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.error = 'Image size must not exceed 2MB.';
      return;
    }

    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.uploadProfilePicture(file).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.profileData = res.data;
          this.message = 'Profile picture updated successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  deleteProfilePicture(): void {
    if (!confirm('Are you sure you want to remove your profile picture?')) return;

    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.deleteProfilePicture().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          if (this.profileData) this.profileData.profilePictureUrl = null;
          this.message = 'Profile picture removed successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  // ==========================================
  // ADDRESS MANAGEMENT
  // ==========================================
  loadAddresses(): void {
    this.loading = true;
    this.api.addresses().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.addressesList = res.data;
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  openAddAddressModal(): void {
    this.isEditingAddress = false;
    this.editingAddressId = null;
    this.resetAddressForm();
    this.isAddressModalOpen = true;
  }

  openEditAddressModal(addr: UserAddressDto): void {
    this.isEditingAddress = true;
    this.editingAddressId = addr.id;
    this.isAddressModalOpen = true;

    this.addrFullName = addr.fullName;
    this.addrMobileNumber = addr.mobileNumber;
    this.addrAlternateMobile = addr.alternateMobile || '';
    this.addrCountry = addr.country;
    this.addrState = addr.state;
    this.addrCity = addr.city;
    this.addrArea = addr.area;
    this.addrLandmark = addr.landmark || '';
    this.addrPostalCode = addr.postalCode;
    this.addrAddressLine1 = addr.addressLine1;
    this.addrAddressLine2 = addr.addressLine2 || '';
    this.addrAddressType = addr.addressType;
    this.addrIsDefault = addr.isDefault;
  }

  closeAddressModal(): void {
    this.isAddressModalOpen = false;
    this.resetAddressForm();
  }

  resetAddressForm(): void {
    this.addrFullName = '';
    this.addrMobileNumber = '';
    this.addrAlternateMobile = '';
    this.addrCountry = 'India';
    this.addrState = '';
    this.addrCity = '';
    this.addrArea = '';
    this.addrLandmark = '';
    this.addrPostalCode = '';
    this.addrAddressLine1 = '';
    this.addrAddressLine2 = '';
    this.addrAddressType = 'Home';
    this.addrIsDefault = false;
  }

  saveAddress(): void {
    this.error = '';
    this.message = '';
    this.loading = true;

    const payload = {
      fullName: this.addrFullName,
      mobileNumber: this.addrMobileNumber,
      alternateMobile: this.addrAlternateMobile || null,
      country: this.addrCountry,
      state: this.addrState,
      city: this.addrCity,
      area: this.addrArea,
      landmark: this.addrLandmark || null,
      postalCode: this.addrPostalCode,
      addressLine1: this.addrAddressLine1,
      addressLine2: this.addrAddressLine2 || null,
      addressType: this.addrAddressType,
      isDefault: this.addrIsDefault
    };

    const request$ = this.isEditingAddress && this.editingAddressId
      ? this.api.updateAddress(this.editingAddressId, payload)
      : this.api.addAddress(payload);

    request$.subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.closeAddressModal();
          this.loadAddresses();
          this.message = this.isEditingAddress ? 'Address updated successfully.' : 'Address added successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  deleteAddress(id: string): void {
    if (!confirm('Are you sure you want to delete this address?')) return;

    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.deleteAddress(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.loadAddresses();
          this.message = 'Address deleted successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  setDefaultAddress(id: string): void {
    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.setDefaultAddress(id).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.loadAddresses();
          this.message = 'Default address updated successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  // ==========================================
  // PASSWORD MANAGEMENT
  // ==========================================
  onPasswordChange(val: string): void {
    this.checkPasswordStrength(val);
  }

  private checkPasswordStrength(val: string): void {
    if (!val) {
      this.strengthScore = 0;
      this.strengthLabel = 'Too Weak';
      this.strengthClass = 'strength-0';
      return;
    }

    let score = 0;
    if (val.length >= 8) score++;
    if (/[A-Z]/.test(val)) score++;
    if (/[a-z]/.test(val)) score++;
    if (/[0-9]/.test(val)) score++;
    if (/[^a-zA-Z0-9]/.test(val)) score++;

    this.strengthScore = score;
    switch (score) {
      case 0:
      case 1:
      case 2:
        this.strengthLabel = 'Weak';
        this.strengthClass = 'strength-1';
        break;
      case 3:
        this.strengthLabel = 'Fair';
        this.strengthClass = 'strength-2';
        break;
      case 4:
        this.strengthLabel = 'Good';
        this.strengthClass = 'strength-3';
        break;
      case 5:
        this.strengthLabel = 'Strong';
        this.strengthClass = 'strength-4';
        break;
    }
  }

  changePassword(): void {
    this.error = '';
    this.message = '';

    if (this.newPassword !== this.confirmPassword) {
      this.error = 'New passwords do not match.';
      return;
    }

    this.loading = true;
    this.api.changePassword({
      currentPassword: this.currentPassword,
      newPassword: this.newPassword,
      confirmPassword: this.confirmPassword
    }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.message = 'Password changed successfully.';
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmPassword = '';
          this.strengthScore = 0;
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  // ==========================================
  // ORDER HISTORY
  // ==========================================
  loadOrders(): void {
    this.loading = true;
    this.api.profileOrders().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.ordersList = res.data;
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  toggleOrderDetails(orderId: string): void {
    if (this.expandedOrderId === orderId) {
      this.expandedOrderId = null;
    } else {
      this.expandedOrderId = orderId;
    }
  }

  downloadInvoice(order: Order): void {
    // Generate text/csv representation as a downloadable invoice file
    const border = '='.repeat(50);
    const invoiceContent = `
${border}
                  SHOPNEST INVOICE
${border}
Order Number : ${order.orderNumber}
Order Status : ${order.status}
Total Amount : INR ${order.totalAmount.toFixed(2)}
Ship Address : ${order.shippingAddress}

Items:
${order.items.map(item => `- ${item.productName} (x${item.quantity}) @ INR ${item.unitPrice.toFixed(2)} = INR ${item.lineTotal.toFixed(2)}`).join('\n')}

${border}
               Thank you for shopping!
${border}
`;
    const blob = new Blob([invoiceContent], { type: 'text/plain;charset=utf-8' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `Invoice_${order.orderNumber}.txt`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  // ==========================================
  // WISHLIST MANAGEMENT
  // ==========================================
  loadWishlist(): void {
    this.loading = true;
    this.api.wishlist().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          this.wishlistItems = res.data;
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  removeFromWishlist(productId: string): void {
    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.removeFromWishlist(productId).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.loadWishlist();
          this.message = 'Item removed from wishlist.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  moveWishlistToCart(productId: string): void {
    this.error = '';
    this.message = '';
    this.loading = true;

    this.api.moveWishlistToCart(productId).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.loadWishlist();
          this.message = 'Item moved to cart successfully.';
        }
      },
      error: (err) => this.handleError(err)
    });
  }

  // ==========================================
  // REVIEWS MANAGEMENT
  // ==========================================
  reviewsList: any[] = [];
  editingReview: any | null = null;
  editReviewRating = 5;
  editReviewTitle = '';
  editReviewDescription = '';

  loadReviews(): void {
    this.loading = true;
    const userId = this.api.currentUser()?.userId;
    if (!userId) {
      this.loading = false;
      return;
    }
    this.api.getReviews(undefined, userId).subscribe({
      next: (res) => {
        this.loading = false;
        this.reviewsList = res.items;
      },
      error: (err) => this.handleError(err)
    });
  }

  openEditReviewModal(review: any): void {
    this.editingReview = review;
    this.editReviewRating = review.rating;
    this.editReviewTitle = review.reviewTitle;
    this.editReviewDescription = review.reviewDescription;
  }

  closeEditReviewModal(): void {
    this.editingReview = null;
  }

  saveReviewEdit(): void {
    if (!this.editingReview) return;
    this.loading = true;
    this.api.updateReview(this.editingReview.id, {
      rating: this.editReviewRating,
      reviewTitle: this.editReviewTitle,
      reviewDescription: this.editReviewDescription,
      isRecommended: this.editingReview.isRecommended
    }).subscribe({
      next: () => {
        this.loading = false;
        this.closeEditReviewModal();
        this.loadReviews();
        this.message = 'Review updated successfully.';
      },
      error: (err) => this.handleError(err)
    });
  }

  deleteReview(id: string): void {
    if (!confirm('Are you sure you want to delete this review?')) return;
    this.loading = true;
    this.api.deleteReview(id).subscribe({
      next: () => {
        this.loading = false;
        this.loadReviews();
        this.message = 'Review deleted successfully.';
      },
      error: (err) => this.handleError(err)
    });
  }

  // ==========================================
  // HELPERS
  // ==========================================
  private handleError(err: any): void {
    this.loading = false;
    if (typeof err === 'object' && err && 'error' in err) {
      const body = err.error;
      if (body?.errors && body.errors.length > 0) {
        this.error = body.errors[0];
        return;
      }
      this.error = body?.error || 'Request failed.';
      return;
    }
    this.error = 'Request failed.';
  }
}
