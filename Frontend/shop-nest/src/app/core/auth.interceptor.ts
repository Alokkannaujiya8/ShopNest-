import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const raw = localStorage.getItem('shopnest.session');
  const token = raw ? (JSON.parse(raw) as { accessToken?: string }).accessToken : null;

  if (!token) return next(request);

  return next(
    request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    }),
  );
};
