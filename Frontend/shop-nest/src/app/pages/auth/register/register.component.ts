import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CustomValidators } from '../../../core/validators';

@Component({
  selector: 'app-auth-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly loading = signal(false);

  // Password strength meter variables
  readonly strengthScore = signal(0);
  readonly strengthLabel = signal('Too Weak');
  readonly strengthClass = signal('strength-0');

  readonly registerForm = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    mobileNumber: ['', [Validators.required, CustomValidators.mobileNumber()]],
    password: ['', [Validators.required, Validators.minLength(8), CustomValidators.passwordStrength()]],
    confirmPassword: ['', [Validators.required]],
    acceptTerms: [false, [Validators.requiredTrue]],
    role: ['Customer']
  }, {
    validators: [CustomValidators.passwordMatch('password', 'confirmPassword')]
  });

  onPasswordInput(val: string): void {
    if (!val) {
      this.strengthScore.set(0);
      this.strengthLabel.set('Too Weak');
      this.strengthClass.set('strength-0');
      return;
    }

    let score = 0;
    if (val.length >= 8) score++;
    if (/[A-Z]/.test(val)) score++;
    if (/[a-z]/.test(val)) score++;
    if (/[0-9]/.test(val)) score++;
    if (/[^a-zA-Z0-9]/.test(val)) score++;

    this.strengthScore.set(score);
    switch (score) {
      case 0:
      case 1:
      case 2:
        this.strengthLabel.set('Weak');
        this.strengthClass.set('strength-1');
        break;
      case 3:
        this.strengthLabel.set('Fair');
        this.strengthClass.set('strength-2');
        break;
      case 4:
        this.strengthLabel.set('Good');
        this.strengthClass.set('strength-3');
        break;
      case 5:
        this.strengthLabel.set('Strong');
        this.strengthClass.set('strength-4');
        break;
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const payload = this.registerForm.getRawValue();

    this.api.register(payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toast.success('Registration successful. OTP verification code has been dispatched.');
          void this.router.navigate(['/auth/verify-otp'], { queryParams: { email: payload.email } });
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
      return err.error.message || err.error.error || 'Registration failed.';
    }
    return 'Registration failed.';
  }
}
