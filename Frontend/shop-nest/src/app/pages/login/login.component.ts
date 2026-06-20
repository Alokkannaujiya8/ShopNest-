import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  mode: 'login' | 'register' = 'login';
  fullName = '';
  email = '';
  password = '';
  role = 'Customer';
  error = '';
  loading = false;

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  submit(): void {
    this.error = '';
    this.loading = true;
    const request =
      this.mode === 'login'
        ? this.api.login(this.email, this.password)
        : this.api.register(this.fullName, this.email, this.password, this.role);

    request.subscribe({
      next: (session) => void this.router.navigateByUrl(session.role === 'Admin' ? '/admin' : '/catalog'),
      error: (err: unknown) => {
        this.error = this.readError(err);
        this.loading = false;
      },
      complete: () => (this.loading = false),
    });
  }

  private readError(err: unknown): string {
    if (typeof err === 'object' && err && 'error' in err) {
      const body = (err as { error?: { error?: string } }).error;
      return body?.error ?? 'Request failed.';
    }
    return 'Request failed.';
  }
}
