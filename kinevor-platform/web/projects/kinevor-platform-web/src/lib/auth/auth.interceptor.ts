import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { I18nService } from '../i18n/i18n.service';

/**
 * Adds `Accept-Language` and, when available, the bearer token to every
 * request; on a 401 outside the auth endpoints it refreshes the token once
 * and replays the request, logging out when the refresh fails.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth  = inject(AuthService);
  const i18n  = inject(I18nService);
  const token = auth.getAccessToken();
  const base = req.clone({
    setHeaders: {
      'Accept-Language': i18n.locale(),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  });
  return next(base).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !req.url.includes('/auth/')) {
        return auth.refreshToken().pipe(
          switchMap(() => {
            const t = auth.getAccessToken();
            return next(t ? base.clone({ setHeaders: { Authorization: `Bearer ${t}` } }) : base);
          }),
          catchError(e => { auth.logout(); return throwError(() => e); }),
        );
      }
      return throwError(() => err);
    }),
  );
};
