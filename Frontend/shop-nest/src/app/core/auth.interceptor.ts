import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const raw = localStorage.getItem('shopnest.session');
  const token = raw ? (JSON.parse(raw) as { accessToken?: string }).accessToken : null;

  const headers: Record<string, string> = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  return next(
    request.clone({
      setHeaders: headers,
      withCredentials: true,
    }),
  );
};
