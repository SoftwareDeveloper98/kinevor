import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Adds the DemoApi trust headers so the sample host can authenticate requests.
 * Headers are only attached to calls under `/api/`.
 */
export const demoApiInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.includes('/api/')) return next(req);

  return next(
    req.clone({
      setHeaders: {
        'X-Demo-Role': 'Staff',
        'X-Demo-Email': 'staff.demo@kinevor.local',
      },
    }),
  );
};
