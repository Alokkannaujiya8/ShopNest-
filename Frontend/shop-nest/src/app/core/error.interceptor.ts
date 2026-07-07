import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { ToastService } from './toast.service';
import { ApiService } from './api.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const api = inject(ApiService);

  return next(req).pipe(
    catchError(err => {
      let errorMessage = 'An unexpected error occurred.';

      if (err.error) {
        if (typeof err.error === 'string') {
          errorMessage = err.error;
        } else if (err.error.message) {
          errorMessage = err.error.message;
        } else if (err.error.errors && Array.isArray(err.error.errors) && err.error.errors.length > 0) {
          errorMessage = err.error.errors.join(', ');
        }
      } else if (err.statusText) {
        errorMessage = err.statusText;
      }

      switch (err.status) {
        case 400:
          toast.warning(errorMessage || 'Bad Request');
          break;
        case 401:
          toast.error('Session expired. Please log in again.');
          api.logout();
          void router.navigate(['/login']);
          break;
        case 403:
          toast.error('Access Denied. You do not have permission to view this resource.');
          void router.navigate(['/catalog']);
          break;
        case 404:
          toast.warning(errorMessage || 'Requested resource not found.');
          break;
        case 500:
          toast.error('Internal Server Error. Please contact support.');
          break;
        default:
          toast.error(errorMessage || 'Network connection error.');
          break;
      }

      return throwError(() => err);
    })
  );
};
