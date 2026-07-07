import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-auth-forgot-password',
  standalone: false,
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly loading = signal(false);

  readonly forgotForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]]
  });

  onSubmit(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    const { email } = this.forgotForm.getRawValue();
    this.loading.set(true);

    this.api.forgotPassword(email).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toast.success('Password reset OTP code has been sent to your email address.');
          void this.router.navigate(['/auth/reset-password'], { queryParams: { email } });
        }
      },
      error: (err) => {
        this.loading.set(false);
        const errMsg = this.readError(err);
        this.toast.error(errMsg);
      }
    });
  }

  private readError(err: any): string {
    if (err && err.error) {
      if (err.error.errors && err.error.errors.length > 0) return err.error.errors[0];
      return err.error.message || err.error.error || 'Request failed.';
    }
    return 'Request failed.';
  }
}
