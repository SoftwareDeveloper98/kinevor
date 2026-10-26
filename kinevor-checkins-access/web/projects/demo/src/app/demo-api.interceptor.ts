import { HttpInterceptorFn } from '@angular/common/http';

/** Adds the demo staff identity headers expected by the standalone API host. */
export const demoApiInterceptor: HttpInterceptorFn = (request, next) => {
  const authenticated = request.clone({
    setHeaders: {
      'X-Demo-Role': 'Staff',
      'X-Demo-User': 'staff-riley',
      'X-Demo-Email': 'riley.chen@kinevor.demo',
    },
  });

  return next(authenticated);
};
