import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../../core/api.service';
import { ToastService } from '../../../core/toast.service';

@Component({
  selector: 'app-auth-verify-otp',
  standalone: false,
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.scss']
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly email = signal('');
  readonly loading = signal(false);
  readonly otpDigits = signal(['', '', '', '', '', '']);

  // Timer properties
  readonly countdown = signal(300); // 5 minutes in seconds
  private timerInterval: any = null;
  readonly canResend = computed(() => this.countdown() === 0);

  readonly formattedTime = computed(() => {
    const minutes = Math.floor(this.countdown() / 60);
    const seconds = this.countdown() % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email.set(params['email'] || '');
      if (!this.email()) {
        this.toast.warning('No email provided for verification. Redirecting...');
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

  onInput(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;
    
    // If typing over an existing character, take the last typed digit
    if (value.length > 1) {
      value = value.substring(value.length - 1);
      input.value = value;
    }
    
    // Accept only numbers
    if (value && !/^\d$/.test(value)) {
      input.value = '';
      return;
    }

    const currentDigits = [...this.otpDigits()];
    currentDigits[index] = value;
    this.otpDigits.set(currentDigits);

    // Auto move to next input box with a brief timeout to prevent keystroke event duplication/leak
    if (value && index < 5) {
      setTimeout(() => {
        const nextInput = document.getElementById(`otp-${index + 1}`) as HTMLInputElement;
        nextInput?.focus();
      }, 0);
    }
  }

  onKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace') {
      if (!this.otpDigits()[index] && index > 0) {
        // Auto move backward and clear previous box
        const currentDigits = [...this.otpDigits()];
        currentDigits[index - 1] = '';
        this.otpDigits.set(currentDigits);

        setTimeout(() => {
          const prevInput = document.getElementById(`otp-${index - 1}`) as HTMLInputElement;
          prevInput?.focus();
        }, 0);
      }
    } else if (/^\d$/.test(event.key)) {
      event.preventDefault();

      const currentDigits = [...this.otpDigits()];
      currentDigits[index] = event.key;
      this.otpDigits.set(currentDigits);

      if (index < 5) {
        setTimeout(() => {
          const nextInput = document.getElementById(`otp-${index + 1}`) as HTMLInputElement;
          nextInput?.focus();
        }, 0);
      }
    }
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const data = event.clipboardData?.getData('text') || '';
    if (!/^\d{6}$/.test(data)) return;

    const digits = data.split('');
    this.otpDigits.set(digits);

    // Focus last input box
    const lastInput = document.getElementById('otp-5') as HTMLInputElement;
    lastInput?.focus();
  }

  resendOtp(): void {
    this.loading.set(true);
    this.api.resendEmailOtp(this.email()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          this.toast.success('A new 6-digit OTP code has been dispatched to your email inbox.');
          this.startTimer();
        }
      },
      error: () => this.loading.set(false)
    });
  }

  onSubmit(): void {
    const otp = this.otpDigits().join('');
    if (otp.length < 6) {
      this.toast.warning('Please enter the full 6-digit code.');
      return;
    }

    this.loading.set(true);
    this.api.verifyEmailOtp(this.email(), otp).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.toast.success('Email verified successfully! Session established.');
          this.stopTimer();
          void this.router.navigateByUrl(res.data.role === 'Admin' ? '/admin' : '/catalog');
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
      return err.error.message || err.error.error || 'Verification failed.';
    }
    return 'Verification failed.';
  }
}
