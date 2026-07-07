import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { ApiService } from './api.service';

export const guestGuard: CanActivateFn = () => {
  const api = inject(ApiService);
  const router = inject(Router);

  if (api.currentUser()) {
    void router.navigate(['/catalog']);
    return false;
  }
  return true;
};
