import { Component, OnInit } from '@angular/core';
import { AdminRoleDto, CreateRoleRequest, UpdateRoleRequest } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-roles',
  standalone: false,
  templateUrl: './admin-roles.component.html',
  styleUrl: './admin-roles.component.scss'
})
export class AdminRolesComponent implements OnInit {
  roles: AdminRoleDto[] = [];
  
  // Search & Pagination state
  search = '';
  page = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // Feedback notifications
  notice = '';
  errorMsg = '';

  // Modals state
  showCreateModal = false;
  showEditModal = false;

  // Selected Role
  selectedRole?: AdminRoleDto;

  // Forms payload
  createForm: CreateRoleRequest = {
    name: '',
    displayName: '',
    description: ''
  };

  updateForm: UpdateRoleRequest = {
    displayName: '',
    description: '',
    isActive: true
  };

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.api.getAdminRoles({
      search: this.search,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.roles = res.data.items;
          this.totalCount = res.data.totalCount;
          this.totalPages = res.data.totalPages;
        }
      },
      error: (err) => this.showError('Failed to load roles.')
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadRoles();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadRoles();
    }
  }

  openCreateModal(): void {
    this.createForm = {
      name: '',
      displayName: '',
      description: ''
    };
    this.showCreateModal = true;
  }

  saveNewRole(): void {
    this.api.createAdminRole(this.createForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Role created successfully.');
          this.showCreateModal = false;
          this.loadRoles();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to create role.')
    });
  }

  openEditModal(role: AdminRoleDto): void {
    this.selectedRole = role;
    this.updateForm = {
      displayName: role.displayName,
      description: role.description ?? '',
      isActive: role.isActive
    };
    this.showEditModal = true;
  }

  saveUpdateRole(): void {
    if (!this.selectedRole) return;
    this.api.updateAdminRole(this.selectedRole.id, this.updateForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Role updated successfully.');
          this.showEditModal = false;
          this.loadRoles();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to update role.')
    });
  }

  deleteRole(role: AdminRoleDto): void {
    const isSystemRole = ['Admin', 'Customer', 'Seller', 'SuperAdmin'].includes(role.name);
    if (isSystemRole) {
      this.showError('Predefined system roles cannot be deleted.');
      return;
    }

    if (!confirm(`Are you sure you want to delete the role ${role.displayName}?`)) return;

    this.api.deleteAdminRole(role.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Role deleted/deactivated.');
          this.loadRoles();
        }
      }
    });
  }

  toggleActivation(role: AdminRoleDto): void {
    const request = role.isActive ? this.api.deactivateAdminRole(role.id) : this.api.activateAdminRole(role.id);
    request.subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice(`Role ${role.isActive ? 'deactivated' : 'activated'} successfully.`);
          this.loadRoles();
        }
      }
    });
  }

  restoreRole(roleId: string): void {
    this.api.restoreAdminRole(roleId).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Role restored successfully.');
          this.loadRoles();
        }
      }
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
