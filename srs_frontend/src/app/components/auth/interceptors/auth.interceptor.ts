import { HttpInterceptorFn } from '@angular/common/http';
import { throwError, timeout } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const request = (() => {
    if (typeof localStorage === 'undefined') return req;

    const token = localStorage.getItem('token');
    if (!token) return req;

    return req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
  })();

  return next(request).pipe(
    timeout({
      each: 10000,
      with: () => throwError(() => new Error('The API did not respond. Make sure the backend is running on http://localhost:8080.'))
    })
  );
};
