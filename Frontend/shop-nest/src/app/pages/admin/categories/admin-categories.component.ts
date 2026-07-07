import { Component, OnInit } from '@angular/core';
import { AdminCategoryDto, CategoryNodeDto, CreateCategoryRequest, UpdateCategoryRequest } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-categories',
  standalone: false,
  templateUrl: './admin-categories.component.html',
  styleUrl: './admin-categories.component.scss'
})
export class AdminCategoriesComponent implements OnInit {
  categories: AdminCategoryDto[] = [];
  treeNodes: CategoryNodeDto[] = [];
  parentList: any[] = [];
  
  // Search, Filter & Paging state
  search = '';
  filterActive = '';
  filterFeatured = '';
  filterDeleted = 'false';
  sortBy = 'displayorder';
  descending = false;
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // View state: 'table' or 'tree'
  viewMode: 'table' | 'tree' = 'table';

  // Notices
  notice = '';
  errorMsg = '';

  // Modal overlays
  showCreateModal = false;
  showEditModal = false;
  showUploadModal = false;
  uploadType: 'image' | 'banner' = 'image';

  selectedCategory?: AdminCategoryDto;

  // Form payloads
  createForm: CreateCategoryRequest = {
    name: '',
    description: '',
    shortDescription: '',
    parentId: null,
    displayOrder: 0,
    isFeatured: false,
    metaTitle: '',
    metaDescription: '',
    metaKeywords: ''
  };

  updateForm: UpdateCategoryRequest = {
    name: '',
    description: '',
    shortDescription: '',
    parentId: null,
    displayOrder: 0,
    isFeatured: false,
    isActive: true,
    metaTitle: '',
    metaDescription: '',
    metaKeywords: ''
  };

  // Upload file storage
  selectedFile?: File;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.reloadAll();
  }

  reloadAll(): void {
    this.loadCategoriesTable();
    this.loadCategoryTree();
    this.loadParentDropdown();
  }

  loadCategoriesTable(): void {
    const activeVal = this.filterActive === 'true' ? true : this.filterActive === 'false' ? false : undefined;
    const featVal = this.filterFeatured === 'true' ? true : this.filterFeatured === 'false' ? false : undefined;
    const delVal = this.filterDeleted === 'true' ? true : false;

    this.api.getAdminCategories({
      search: this.search,
      isActive: activeVal,
      isFeatured: featVal,
      isDeleted: delVal,
      sortBy: this.sortBy,
      descending: this.descending,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.categories = res.data.items;
          this.totalCount = res.data.totalCount;
          this.totalPages = res.data.totalPages;
        }
      },
      error: () => this.showError('Failed to load categories.')
    });
  }

  loadCategoryTree(): void {
    this.api.getCategoryTree().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.treeNodes = res.data;
        }
      }
    });
  }

  loadParentDropdown(): void {
    this.api.getAdminCategories({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.parentList = res.data.items;
        }
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadCategoriesTable();
  }

  onSort(col: string): void {
    if (this.sortBy === col) {
      this.descending = !this.descending;
    } else {
      this.sortBy = col;
      this.descending = false;
    }
    this.page = 1;
    this.loadCategoriesTable();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadCategoriesTable();
    }
  }

  openCreateModal(): void {
    this.createForm = {
      name: '',
      description: '',
      shortDescription: '',
      parentId: null,
      displayOrder: 0,
      isFeatured: false,
      metaTitle: '',
      metaDescription: '',
      metaKeywords: ''
    };
    this.showCreateModal = true;
  }

  saveNewCategory(): void {
    // Basic validations
    if (!this.createForm.name.trim()) {
      this.showError('Category name is required.');
      return;
    }

    this.api.adminCreateCategory(this.createForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Category created successfully.');
          this.showCreateModal = false;
          this.reloadAll();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to create category. A cycle or duplicate slug might have occurred.')
    });
  }

  openEditModal(category: AdminCategoryDto): void {
    this.selectedCategory = category;
    this.updateForm = {
      name: category.name,
      description: category.description ?? '',
      shortDescription: category.shortDescription ?? '',
      parentId: category.parentId || null,
      displayOrder: category.displayOrder,
      isFeatured: category.isFeatured,
      isActive: category.isActive,
      metaTitle: category.metaTitle ?? '',
      metaDescription: category.metaDescription ?? '',
      metaKeywords: category.metaKeywords ?? ''
    };
    this.showEditModal = true;
  }

  saveUpdateCategory(): void {
    if (!this.selectedCategory) return;
    if (!this.updateForm.name.trim()) {
      this.showError('Category name is required.');
      return;
    }

    this.api.adminUpdateCategory(this.selectedCategory.id, this.updateForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Category updated successfully.');
          this.showEditModal = false;
          this.reloadAll();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to update category. Ensure parent selection does not create cycles.')
    });
  }

  deleteCategory(category: AdminCategoryDto): void {
    if (category.childrenCount > 0) {
      this.showError('Cannot delete a category with active subcategories.');
      return;
    }

    if (!confirm(`Are you sure you want to soft-delete the category ${category.name}?`)) return;

    this.api.deleteCategory(category.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Category soft-deleted.');
          this.reloadAll();
        }
      }
    });
  }

  toggleActivation(category: AdminCategoryDto): void {
    const request = category.isActive ? this.api.deactivateCategory(category.id) : this.api.activateCategory(category.id);
    request.subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice(`Category status toggled.`);
          this.reloadAll();
        }
      }
    });
  }

  restoreCategory(category: AdminCategoryDto): void {
    this.api.restoreCategory(category.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Category restored successfully.');
          this.reloadAll();
        }
      }
    });
  }

  openUploadModal(category: AdminCategoryDto, type: 'image' | 'banner'): void {
    this.selectedCategory = category;
    this.uploadType = type;
    this.selectedFile = undefined;
    this.showUploadModal = true;
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  uploadFile(): void {
    if (!this.selectedCategory || !this.selectedFile) return;

    const request = this.uploadType === 'image'
      ? this.api.uploadCategoryImage(this.selectedCategory.id, this.selectedFile)
      : this.api.uploadCategoryBanner(this.selectedCategory.id, this.selectedFile);

    request.subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice(`${this.uploadType === 'image' ? 'Category Image' : 'Banner'} uploaded successfully.`);
          this.showUploadModal = false;
          this.reloadAll();
        } else {
          this.showError(res.message || 'Upload failed.');
        }
      },
      error: () => this.showError('Upload failed. Ensure file format is valid.')
    });
  }

  private showNotice(msg: string): void {
    this.notice = msg;
    this.errorMsg = '';
    setTimeout(() => (this.notice = ''), 5000);
  }

  private showError(msg: string): void {
    this.errorMsg = msg;
    this.notice = '';
    setTimeout(() => (this.errorMsg = ''), 7000);
  }
}
