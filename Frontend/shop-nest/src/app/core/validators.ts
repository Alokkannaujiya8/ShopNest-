import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  static passwordMatch(passwordKey: string, confirmPasswordKey: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.get(passwordKey);
      const confirmPassword = control.get(confirmPasswordKey);

      if (!password || !confirmPassword) return null;

      if (confirmPassword.errors && !confirmPassword.errors['mustMatch']) {
        return null;
      }

      if (password.value !== confirmPassword.value) {
        confirmPassword.setErrors({ mustMatch: true });
        return { mustMatch: true };
      } else {
        confirmPassword.setErrors(null);
        return null;
      }
    };
  }

  static passwordStrength(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;

      const hasUpperCase = /[A-Z]/.test(value);
      const hasLowerCase = /[a-z]/.test(value);
      const hasNumeric = /[0-9]/.test(value);
      const hasSpecial = /[^A-Za-z0-9]/.test(value);
      const isLengthValid = value.length >= 8;

      const isValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecial && isLengthValid;
      
      return !isValid ? { passwordStrength: true } : null;
    };
  }

  static mobileNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;

      // Matches international phone numbers (e.g., +919876543210 or 9876543210)
      const pattern = /^\+?[1-9]\d{1,14}$/;
      return !pattern.test(value) ? { invalidMobile: true } : null;
    };
  }

  static otp(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value) return null;

      const pattern = /^\d{6}$/;
      return !pattern.test(value) ? { invalidOtp: true } : null;
    };
  }
}
