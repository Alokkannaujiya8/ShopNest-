import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-auth-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly showPassword = signal(false);
  readonly loading = signal(false);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    rememberMe: [false]
  });

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { email, password, rememberMe } = this.loginForm.getRawValue();
    this.loading.set(true);

    this.api.login(email, password, rememberMe).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.toast.success(`Welcome back, ${res.data.fullName}!`);
          void this.router.navigateByUrl(res.data.role === 'Admin' ? '/admin' : '/catalog');
        }
      },
      error: (err) => {
        this.loading.set(false);
        const errMsg = this.readError(err);
        if (errMsg === 'Email is not verified. Please verify your email first.') {
          this.api.resendEmailOtp(email).subscribe({
            next: () => {
              this.toast.info('Verification OTP code has been sent to your email.');
              void this.router.navigate(['/auth/verify-otp'], { queryParams: { email } });
            }
          });
        }
      }
    });
  }

  private readError(err: any): string {
    if (err && err.error) {
      if (err.error.errors && err.error.errors.length > 0) return err.error.errors[0];
      return err.error.message || err.error.error || 'Authentication failed.';
    }
    return 'Authentication failed.';
  }
}
