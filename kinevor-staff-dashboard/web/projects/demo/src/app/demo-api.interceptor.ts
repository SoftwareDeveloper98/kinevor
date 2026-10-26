import { HttpInterceptorFn } from '@angular/common/http';

export const demoApiInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.includes('/api/')) return next(req);

  return next(
    req.clone({
      setHeaders: {
        'X-Demo-Role': 'Staff',
        'X-Demo-Email': 'staff.demo@kinevor.local',
        'X-Demo-User': 'staff-demo',
      },
    }),
  );
};
