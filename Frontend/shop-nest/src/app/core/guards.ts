import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ApiService } from './api.service';

export const authGuard: CanActivateFn = () => {
  const api = inject(ApiService);
  const router = inject(Router);
  if (api.currentUser()) return true;
  return router.createUrlTree(['/auth/login']);
};

export const adminGuard: CanActivateFn = () => {
  const api = inject(ApiService);
  const router = inject(Router);
  if (api.currentUser()?.role === 'Admin') return true;
  return router.createUrlTree(['/catalog']);
};

export { guestGuard } from './guest.guard';
