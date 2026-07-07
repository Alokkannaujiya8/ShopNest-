import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from './loading.service';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loader = inject(LoadingService);
  
  // Skip global loading spinner for GET requests (fetching data)
  if (req.method === 'GET') {
    return next(req);
  }

  loader.show();
  return next(req).pipe(
    finalize(() => loader.hide())
  );
};
