import { Component, OnInit } from '@angular/core';
import { AdminUserDto, AdminRoleDto, CreateAdminUserRequest, UpdateAdminUserRequest } from '../../../core/api.models';
import { ApiService } from '../../../core/api.service';

@Component({
  selector: 'app-admin-users',
  standalone: false,
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss'
})
export class AdminUsersComponent implements OnInit {
  users: AdminUserDto[] = [];
  roles: AdminRoleDto[] = [];
  
  // Search & Filter state
  search = '';
  roleFilter = '';
  sortBy = 'createdat';
  descending = true;
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
  showResetPasswordModal = false;

  // Selected User
  selectedUser?: AdminUserDto;

  // Forms payload
  createForm: CreateAdminUserRequest = {
    fullName: '',
    email: '',
    mobileNumber: '',
    password: '',
    role: 'Customer'
  };

  updateForm: UpdateAdminUserRequest = {
    fullName: '',
    mobileNumber: '',
    role: 'Customer',
    isActive: true
  };

  resetPasswordForm = {
    newPassword: '',
    confirmPassword: '',
    forcePasswordChange: false
  };

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers(): void {
    this.api.getAdminUsers({
      search: this.search,
      role: this.roleFilter,
      sortBy: this.sortBy,
      descending: this.descending,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.users = res.data.items;
          this.totalCount = res.data.totalCount;
          this.totalPages = res.data.totalPages;
        }
      },
      error: (err) => this.showError('Failed to load users.')
    });
  }

  loadRoles(): void {
    this.api.getAdminRoles({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.roles = res.data.items;
        }
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadUsers();
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.descending = !this.descending;
    } else {
      this.sortBy = column;
      this.descending = false;
    }
    this.page = 1;
    this.loadUsers();
  }

  changePage(newPage: number): void {
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadUsers();
    }
  }

  openCreateModal(): void {
    this.createForm = {
      fullName: '',
      email: '',
      mobileNumber: '',
      password: '',
      role: 'Customer'
    };
    this.showCreateModal = true;
  }

  saveNewUser(): void {
    this.api.createAdminUser(this.createForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('User created successfully.');
          this.showCreateModal = false;
          this.loadUsers();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to create user.')
    });
  }

  openEditModal(user: AdminUserDto): void {
    this.selectedUser = user;
    this.updateForm = {
      fullName: user.fullName,
      mobileNumber: user.mobileNumber,
      role: user.role,
      isActive: user.isActive
    };
    this.showEditModal = true;
  }

  saveUpdateUser(): void {
    if (!this.selectedUser) return;
    this.api.updateAdminUser(this.selectedUser.id, this.updateForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('User updated successfully.');
          this.showEditModal = false;
          this.loadUsers();
        } else {
          this.showError(res.message || 'Validation failed.');
        }
      },
      error: (err) => this.showError('Failed to update user.')
    });
  }

  deleteUser(user: AdminUserDto): void {
    if (!confirm(`Are you sure you want to delete ${user.fullName}?`)) return;
    this.api.deleteAdminUser(user.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('User soft-deleted successfully.');
          this.loadUsers();
        }
      }
    });
  }

  toggleLock(user: AdminUserDto): void {
    const request = user.isLocked ? this.api.unlockAdminUser(user.id) : this.api.lockAdminUser(user.id);
    request.subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice(`User account ${user.isLocked ? 'unlocked' : 'locked'} successfully.`);
          this.loadUsers();
        }
      }
    });
  }

  toggleActivation(user: AdminUserDto): void {
    const request = user.isActive ? this.api.deactivateAdminUser(user.id) : this.api.activateAdminUser(user.id);
    request.subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice(`User status ${user.isActive ? 'deactivated' : 'activated'} successfully.`);
          this.loadUsers();
        }
      }
    });
  }

  openResetPasswordModal(user: AdminUserDto): void {
    this.selectedUser = user;
    this.resetPasswordForm = {
      newPassword: '',
      confirmPassword: '',
      forcePasswordChange: true
    };
    this.showResetPasswordModal = true;
  }

  resetPassword(): void {
    if (!this.selectedUser) return;
    if (this.resetPasswordForm.newPassword !== this.resetPasswordForm.confirmPassword) {
      this.showError('Passwords do not match.');
      return;
    }

    this.api.resetAdminUserPassword(this.selectedUser.id, this.resetPasswordForm).subscribe({
      next: (res) => {
        if (res.success) {
          this.showNotice('Password reset successfully.');
          this.showResetPasswordModal = false;
        } else {
          this.showError(res.message || 'Reset failed.');
        }
      },
      error: (err) => this.showError('Failed to reset password.')
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
