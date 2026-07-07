import { Component, inject, signal, OnInit, OnDestroy, computed } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';
import { CustomValidators } from '../../../core/validators';

@Component({
  selector: 'app-auth-reset-password',
  standalone: false,
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly email = signal('');
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly loading = signal(false);

  // Timer properties
  readonly countdown = signal(300);
  private timerInterval: any = null;
  readonly canResend = computed(() => this.countdown() === 0);

  readonly formattedTime = computed(() => {
    const minutes = Math.floor(this.countdown() / 60);
    const seconds = this.countdown() % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  });

  // Password strength meter variables
  readonly strengthScore = signal(0);
  readonly strengthLabel = signal('Too Weak');
  readonly strengthClass = signal('strength-0');

  readonly resetForm = this.fb.nonNullable.group({
    otp: ['', [Validators.required, CustomValidators.otp()]],
    password: ['', [Validators.required, Validators.minLength(8), CustomValidators.passwordStrength()]],
    confirmPassword: ['', [Validators.required]]
  }, {
    validators: [CustomValidators.passwordMatch('password', 'confirmPassword')]
  });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email.set(params['email'] || '');
      if (!this.email()) {
        this.toast.warning('No email provided for password recovery. Redirecting...');
        void this.router.navigate(['/auth/login']);
      }
    });
    this.startTimer();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  startTimer(): void {
    this.stopTimer();
    this.countdown.set(300);
    this.timerInterval = setInterval(() => {
      this.countdown.update(t => Math.max(0, t - 1));
      if (this.countdown() === 0) {
        this.stopTimer();
      }
    }, 1000);
  }

  stopTimer(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
  }

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

  resendOtp(): void {
    this.loading.set(true);
    this.api.forgotPassword(this.email()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toast.success('A new reset OTP code has been sent to your email.');
          this.startTimer();
        }
      },
      error: () => this.loading.set(false)
    });
  }

  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    const { otp, password, confirmPassword } = this.resetForm.getRawValue();
    this.loading.set(true);

    this.api.resetPassword(this.email(), otp, password, confirmPassword).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toast.success('Your password has been successfully reset. Please sign in with your new credentials.');
          this.stopTimer();
          void this.router.navigate(['/auth/login']);
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
      return err.error.message || err.error.error || 'Password reset failed.';
    }
    return 'Password reset failed.';
  }
}
