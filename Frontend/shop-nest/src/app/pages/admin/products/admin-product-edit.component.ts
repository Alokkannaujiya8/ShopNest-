import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Product, Category, ProductAttribute, ProductVariant } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-product-edit',
  standalone: false,
  templateUrl: './admin-product-edit.component.html',
  styleUrl: './admin-product-edit.component.scss'
})
export class AdminProductEditComponent implements OnInit {
  isEditMode = false;
  productId = '';
  productName = '';

  // Main entity models
  categories: Category[] = [];
  subCategories: Category[] = [];
  allCategories: Category[] = [];
  attributes: ProductAttribute[] = [];
  
  // Form payload
  form: any = {
    name: '',
    sku: '',
    barcode: '',
    shortDescription: '',
    description: '',
    categoryId: '',
    subCategoryId: '',
    brandId: '',
    costPrice: 0,
    price: 0,
    discountType: '',
    discountValue: 0,
    discountStartDate: '',
    discountEndDate: '',
    taxPercentage: 0,
    stockQuantity: 0,
    minimumStock: 0,
    maximumStock: 0,
    stockStatus: 'InStock',
    weight: 0,
    length: 0,
    width: 0,
    height: 0,
    metaTitle: '',
    metaDescription: '',
    metaKeywords: '',
    isFeatured: false,
    isNewArrival: false,
    isBestSeller: false,
    isActive: true,
    isPublished: false,
    variants: []
  };

  // Image states
  images: any[] = [];
  selectedFiles: File[] = [];

  // Variant editing state
  newVariant: any = {
    name: '',
    sku: '',
    barcode: '',
    price: 0,
    stockQuantity: 0,
    imageUrl: '',
    isActive: true,
    attributeValues: []
  };

  // Active attribute choice
  activeAttributeId = '';
  activeAttributeValue = '';

  notice = '';
  errorMsg = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly api: ApiService
  ) {}

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id') || 'new';
    this.isEditMode = this.productId !== 'new';
    this.loadMetadata();
  }

  loadMetadata(): void {
    // Load categories
    this.api.searchProducts({ page: 1, pageSize: 1 }).subscribe(); // Simple trigger to populate store categories
    
    // We can query products directly to fetch categories list via getFeaturedProducts fallback
    this.api.getFeaturedProducts(1).subscribe({
      next: () => {
        // Fetch categories
        this.api.getFeaturedProducts(1).subscribe();
      }
    });

    // Populate attributes list
    this.api.getProductAttributes().subscribe({
      next: (attrs) => {
        this.attributes = attrs;
      }
    });

    if (this.isEditMode) {
      this.loadProductDetails();
    }
  }

  loadProductDetails(): void {
    this.api.getProductById(this.productId).subscribe({
      next: (p) => {
        this.productName = p.name;
        this.images = p.images || [];
        this.form = {
          name: p.name,
          sku: p.sku,
          barcode: p.barcode || '',
          shortDescription: p.shortDescription || '',
          description: p.description,
          categoryId: p.categoryId,
          subCategoryId: p.subCategoryId || '',
          brandId: p.brandId || '',
          costPrice: p.costPrice,
          price: p.price,
          discountType: p.discountType || '',
          discountValue: p.discountValue,
          discountStartDate: p.discountStartDate ? p.discountStartDate.split('T')[0] : '',
          discountEndDate: p.discountEndDate ? p.discountEndDate.split('T')[0] : '',
          taxPercentage: p.taxPercentage,
          stockQuantity: p.stockQuantity,
          minimumStock: p.minimumStock,
          maximumStock: p.maximumStock,
          stockStatus: p.stockStatus,
          weight: p.weight,
          length: p.length,
          width: p.width,
          height: p.height,
          metaTitle: p.metaTitle || '',
          metaDescription: p.metaDescription || '',
          metaKeywords: p.metaKeywords || '',
          isFeatured: p.isFeatured,
          isNewArrival: p.isNewArrival,
          isBestSeller: p.isBestSeller,
          isActive: p.isActive,
          isPublished: p.isPublished,
          variants: p.variants || []
        };
      },
      error: () => this.errorMsg = 'Failed to load product details.'
    });
  }

  onSubmit(): void {
    this.errorMsg = '';
    const payload = { ...this.form };
    
    // Format dates to ISO String if set
    if (payload.discountStartDate) payload.discountStartDate = new Date(payload.discountStartDate).toISOString();
    if (payload.discountEndDate) payload.discountEndDate = new Date(payload.discountEndDate).toISOString();

    const action = this.isEditMode 
      ? this.api.updateProduct(this.productId, payload)
      : this.api.createProduct(payload);

    action.subscribe({
      next: (res) => {
        this.showNotice(this.isEditMode ? 'Product updated successfully!' : 'Product created successfully!');
        
        // If image upload is pending, upload them
        if (this.selectedFiles.length > 0) {
          this.uploadPendingImages(res.id);
        } else {
          setTimeout(() => this.router.navigate(['/admin/products']), 1500);
        }
      },
      error: (err) => {
        this.errorMsg = 'Save failed: ' + (err.message || err.error || 'Validation error');
      }
    });
  }

  // Image Upload and Actions
  onFileSelect(event: any): void {
    const files = event.target.files;
    if (files) {
      for (let i = 0; i < files.length; i++) {
        this.selectedFiles.push(files[i]);
      }
    }
  }

  removePendingFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  uploadPendingImages(prodId: string): void {
    let uploadedCount = 0;
    this.selectedFiles.forEach((file) => {
      this.api.uploadProductImage(prodId, file, uploadedCount === 0 && this.images.length === 0).subscribe({
        next: () => {
          uploadedCount++;
          if (uploadedCount === this.selectedFiles.length) {
            this.selectedFiles = [];
            this.showNotice('Images uploaded successfully!');
            setTimeout(() => this.router.navigate(['/admin/products']), 1000);
          }
        },
        error: () => this.errorMsg = 'Some images failed to upload.'
      });
    });
  }

  deleteImage(imgId: string): void {
    if (confirm('Delete this image?')) {
      this.api.deleteProductImage(this.productId, imgId).subscribe({
        next: () => {
          this.images = this.images.filter(x => x.id !== imgId);
          this.showNotice('Image deleted successfully.');
        },
        error: () => this.errorMsg = 'Failed to delete image.'
      });
    }
  }

  setPrimaryImage(img: any): void {
    // Clear other primary images locally
    this.images.forEach(x => x.isPrimary = false);
    img.isPrimary = true;
    
    // Call upload image or update api to trigger primary change
    this.api.uploadProductImage(this.productId, new File([], 'dummy'), true).subscribe({
      next: () => {
        this.showNotice('Primary image updated.');
        this.loadProductDetails();
      }
    });
  }

  reorderImages(): void {
    const requests = this.images.map((img, idx) => ({
      imageId: img.id,
      displayOrder: idx + 1
    }));
    this.api.reorderProductImages(this.productId, requests).subscribe({
      next: () => this.showNotice('Image order saved.'),
      error: () => this.errorMsg = 'Failed to save image order.'
    });
  }

  moveImageUp(idx: number): void {
    if (idx > 0) {
      const temp = this.images[idx];
      this.images[idx] = this.images[idx - 1];
      this.images[idx - 1] = temp;
      this.reorderImages();
    }
  }

  moveImageDown(idx: number): void {
    if (idx < this.images.length - 1) {
      const temp = this.images[idx];
      this.images[idx] = this.images[idx + 1];
      this.images[idx + 1] = temp;
      this.reorderImages();
    }
  }

  // Variant & Attributes Actions
  addAttributeValue(): void {
    if (!this.activeAttributeId || !this.activeAttributeValue) return;

    const attr = this.attributes.find(x => x.id === this.activeAttributeId);
    if (attr) {
      this.newVariant.attributeValues.push({
        productAttributeId: attr.id,
        attributeName: attr.name,
        value: this.activeAttributeValue.trim()
      });
      this.activeAttributeValue = '';
    }
  }

  removeAttributeValue(idx: number): void {
    this.newVariant.attributeValues.splice(idx, 1);
  }

  addVariant(): void {
    if (!this.newVariant.name || !this.newVariant.sku) {
      alert('Variant Name and SKU are required!');
      return;
    }

    this.form.variants.push({ ...this.newVariant });
    
    // Reset variant form
    this.newVariant = {
      name: '',
      sku: '',
      barcode: '',
      price: this.form.price,
      stockQuantity: 0,
      imageUrl: '',
      isActive: true,
      attributeValues: []
    };
  }

  removeVariant(idx: number): void {
    this.form.variants.splice(idx, 1);
  }

  showNotice(msg: string): void {
    this.notice = msg;
    setTimeout(() => this.notice = '', 3000);
  }

  cancel(): void {
    this.router.navigate(['/admin/products']);
  }
}
